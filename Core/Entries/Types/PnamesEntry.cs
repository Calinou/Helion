﻿using Helion.Resources;
using Helion.Resources.Definitions.Texture;
using NLog;

namespace Helion.Entries.Types
{
    /// <summary>
    /// An entry that contains pnames information.
    /// </summary>
    public class PnamesEntry : Entry
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The pnames data.
        /// </summary>
        /// <remarks>
        /// If the entry was corrupt, it will be an empty set of data.
        /// </remarks>
        public Pnames Pnames { get; } = new Pnames();

        public PnamesEntry(EntryId id, EntryPath path, byte[] data, ResourceNamespace resourceNamespace) :
            base(id, path, data, resourceNamespace)
        {
            Pnames? pnames = Pnames.From(data);
            if (pnames != null)
                Pnames = pnames;
            else
            {
                log.Warn($"Corrupt Pnames at: {Path}");
                Corrupt = true;
            }
        }

        public override ResourceType GetResourceType() => ResourceType.Pnames;
    }
}
