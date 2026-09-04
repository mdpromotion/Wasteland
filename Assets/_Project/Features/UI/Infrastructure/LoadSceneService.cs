using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace _Project.Features.UI.Infrastructure
{
    public class LoadSceneService : ILoadSceneService
    {
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _handles = new();

        public bool IsLoaded(string sceneAddress) => _handles.ContainsKey(sceneAddress);

        public async UniTask LoadAdditiveAsync(string sceneAddress, IProgress<float> progress = null)
        {
            if (string.IsNullOrEmpty(sceneAddress))
                throw new ArgumentException("Scene address must not be null or empty.", nameof(sceneAddress));

            if (_handles.ContainsKey(sceneAddress))
            {
                Debug.LogWarning($"[LoadSceneService] Scene already loaded, skipping: {sceneAddress}");
                progress?.Report(1f);
                return;
            }

            AsyncOperationHandle<SceneInstance> handle =
                Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive, activateOnLoad: true);

            while (!handle.IsDone)
            {
                progress?.Report(handle.PercentComplete);
                await UniTask.Yield();
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[LoadSceneService] Failed to load scene by address: {sceneAddress}");
                Addressables.Release(handle);
                throw handle.OperationException ?? new Exception("Failed to load scene via Addressables.");
            }

            _handles[sceneAddress] = handle;
            SceneManager.SetActiveScene(handle.Result.Scene);

            progress?.Report(1f);
        }

        public async UniTask UnloadAsync(string sceneAddress)
        {
            if (string.IsNullOrEmpty(sceneAddress))
                return;

            if (_handles.TryGetValue(sceneAddress, out var handle))
            {
                await Addressables.UnloadSceneAsync(handle).Task;
                _handles.Remove(sceneAddress);
                return;
            }

            Scene scene = SceneManager.GetSceneByPath(sceneAddress);

            if (!scene.IsValid())
                scene = SceneManager.GetSceneByName(GetSceneNameFromAddress(sceneAddress));

            if (scene.IsValid() && scene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(scene);
            }
        }

        private static string GetSceneNameFromAddress(string sceneAddress)
        {
            int lastSlash = sceneAddress.LastIndexOf('/');
            string fileName = lastSlash >= 0 ? sceneAddress[(lastSlash + 1)..] : sceneAddress;
            int dot = fileName.LastIndexOf('.');
            return dot >= 0 ? fileName[..dot] : fileName;
        }
    }
}