using System.Collections.Generic;
using _Project.Features.Items.Application;

namespace _Project.Features.Items.Presentation
{
    /// <summary>
    /// Pure selection of the pickups that may show a pickup prompt, by distance.
    /// Deliberately side-effect free so it is unit testable and allocation-free
    /// when <paramref name="visible"/> is reused between ticks.
    /// </summary>
    public static class PickupPromptSelection
    {
        /// <summary>
        /// Fills <paramref name="visible"/> with at most <paramref name="maxVisible"/>
        /// nearest candidates, ascending by distance. Clears the list first.
        /// Entries with a non-finite distance are skipped.
        /// </summary>
        public static void SelectNearest(
            IReadOnlyCollection<IItemPickupTarget> candidates,
            int maxVisible,
            List<IItemPickupTarget> visible)
        {
            if (visible == null)
                throw new System.ArgumentNullException(nameof(visible));

            visible.Clear();

            if (candidates == null || maxVisible <= 0 || candidates.Count == 0)
                return;

            foreach (IItemPickupTarget candidate in candidates)
            {
                if (candidate == null)
                    continue;

                float distance = candidate.Distance;
                if (float.IsInfinity(distance) || float.IsNaN(distance) || distance < 0f)
                    continue;

                int index = FindInsertIndex(visible, distance);
                if (index >= maxVisible)
                    continue;

                visible.Insert(index, candidate);
                if (visible.Count > maxVisible)
                    visible.RemoveAt(maxVisible);
            }
        }

        private static int FindInsertIndex(List<IItemPickupTarget> visible, float distance)
        {
            for (int i = 0; i < visible.Count; i++)
                if (distance < visible[i].Distance)
                    return i;

            return visible.Count;
        }
    }
}
