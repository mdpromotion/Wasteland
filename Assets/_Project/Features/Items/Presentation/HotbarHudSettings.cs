using UnityEngine;

namespace _Project.Features.Items.Presentation
{
    [CreateAssetMenu(menuName = "Project/UI/Inventory HUD Settings", fileName = "HotbarHudSettings")]
    public sealed class HotbarHudSettings : ScriptableObject
    {
        [Header("Layout")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
        [SerializeField] private Vector2 hotbarSize = new Vector2(720f, 98f);
        [SerializeField] private Vector2 slotSize = new Vector2(72f, 82f);
        [SerializeField] private float slotSpacing = 7f;
        [SerializeField] private float bottomOffset = 20f;
        [SerializeField] private float selectionToastOffset = 124f;
        [SerializeField] private float heldItemLabelRightOffset = 24f;
        [SerializeField] private float heldItemLabelBottomOffset = 24f;
        [SerializeField] private Vector2 heldItemPanelSize = new Vector2(360f, 240f);

        [Header("Style")]
        [SerializeField] private Font font;
        [SerializeField] private Color slotColor = new Color(0.08f, 0.1f, 0.12f, 0.88f);
        [SerializeField] private Color selectedSlotColor = new Color(0.16f, 0.2f, 0.22f, 0.96f);
        [SerializeField] private Color highlightColor = new Color(0.95f, 0.72f, 0.24f, 1f);
        [SerializeField] private int keyFontSize = 12;
        [SerializeField] private int countFontSize = 12;
        [SerializeField] private int toastFontSize = 24;
        [SerializeField] private int heldItemFontSize = 20;

        [Header("Selection Toast")]
        [SerializeField, Min(0f)] private float selectionToastDuration = 2.5f;
        [SerializeField, Min(0f)] private float selectionToastFadeDuration = 0.45f;

        [Header("Held Item")]
        [SerializeField] private Vector3 heldItemPosition = new Vector3(0.42f, -0.34f, 0.72f);
        [SerializeField] private Vector3 heldItemRotation = new Vector3(12f, -18f, 6f);
        [SerializeField] private Vector3 heldItemScale = new Vector3(0.18f, 0.18f, 0.5f);
        [SerializeField] private Vector3 stowedItemPosition = new Vector3(0.42f, -0.9f, 0.35f);
        [SerializeField, Min(0.01f)] private float equipDuration = 0.28f;

        [Header("Pickup Billboard")]
        [SerializeField] private PickupBillboardTemplate pickupBillboardPrefab;
        [SerializeField] private Vector2 pickupBillboardSize = new Vector2(310f, 96f);
        [SerializeField, Min(0.001f)] private float pickupBillboardWorldScale = 0.005f;
        [SerializeField] private Vector3 pickupBillboardOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] private float pickupIconSize = 54f;
        [SerializeField] private int pickupTitleFontSize = 20;
        [SerializeField] private int pickupPromptFontSize = 16;
        [SerializeField] private string pickupPromptText = "Pick up";

        public Vector2 ReferenceResolution => referenceResolution;
        public Vector2 HotbarSize => hotbarSize;
        public Vector2 SlotSize => slotSize;
        public float SlotSpacing => slotSpacing;
        public float BottomOffset => bottomOffset;
        public float SelectionToastOffset => selectionToastOffset;
        public float HeldItemLabelRightOffset => heldItemLabelRightOffset;
        public float HeldItemLabelBottomOffset => heldItemLabelBottomOffset;
        public Vector2 HeldItemPanelSize => heldItemPanelSize;
        public Font Font => font;
        public Color SlotColor => slotColor;
        public Color SelectedSlotColor => selectedSlotColor;
        public Color HighlightColor => highlightColor;
        public int KeyFontSize => keyFontSize;
        public int CountFontSize => countFontSize;
        public int ToastFontSize => toastFontSize;
        public int HeldItemFontSize => heldItemFontSize;
        public float SelectionToastDuration => selectionToastDuration;
        public float SelectionToastFadeDuration => selectionToastFadeDuration;
        public Vector3 HeldItemPosition => heldItemPosition;
        public Vector3 HeldItemRotation => heldItemRotation;
        public Vector3 HeldItemScale => heldItemScale;
        public Vector3 StowedItemPosition => stowedItemPosition;
        public float EquipDuration => equipDuration;
        public PickupBillboardTemplate PickupBillboardPrefab => pickupBillboardPrefab;
        public Vector2 PickupBillboardSize => pickupBillboardSize;
        public float PickupBillboardWorldScale => pickupBillboardWorldScale;
        public Vector3 PickupBillboardOffset => pickupBillboardOffset;
        public float PickupIconSize => pickupIconSize;
        public int PickupTitleFontSize => pickupTitleFontSize;
        public int PickupPromptFontSize => pickupPromptFontSize;
        public string PickupPromptText => pickupPromptText;
    }
}