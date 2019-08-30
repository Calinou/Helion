using System.Collections.Generic;
using Helion.Resources.Archives.Collection;
using Helion.Resources.Definitions.Decorate;
using Helion.Util;
using Helion.Util.Container;
using MoreLinq;
using NLog;

namespace Helion.World.Entities.Definition.Composer
{
    /// <summary>
    /// Responsible for building up entity definitions from existing decorate
    /// definitions.
    /// </summary>
    /// <remarks>
    /// We cannot use the decorate definitions directly because they may be
    /// missing important data we need, and the performance hit of checking
    /// for nulls or missing values frequently would add up. Further, they
    /// lack the information from parent to child (they only hold their own
    /// data and leave it up to someone else to piece everything together).
    /// All of the inheritance data needs to be compiled into one final class.
    /// This type is the one responsible for that.
    /// </remarks>
    public class EntityDefinitionComposer
    {
        private const int RecursiveDefinitionOverflow = 10000;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ArchiveCollection m_archiveCollection;
        private readonly AvailableIndexTracker m_indexTracker = new AvailableIndexTracker();
        private readonly Dictionary<CIString, EntityDefinition> m_definitions = new Dictionary<CIString, EntityDefinition>();

        public EntityDefinitionComposer(ArchiveCollection archiveCollection)
        {
            m_archiveCollection = archiveCollection;
        }

        public EntityDefinition? Get(CIString name)
        {
            if (m_definitions.TryGetValue(name, out EntityDefinition definition))
                return definition;
            return ComposeNewDefinition(name);
        }

        private static void ApplyActorComponents(EntityDefinition definition, ActorDefinition actorDefinition)
        {
            DefinitionFlagApplier.Apply(definition, actorDefinition.Flags);
            DefinitionPropertyApplier.Apply(definition, actorDefinition.Properties);
            DefinitionStateApplier.Apply(definition, actorDefinition.States);
        }

        private bool CreateInheritanceOrderedList(ActorDefinition actorDef, out LinkedList<ActorDefinition> definitions)
        {
            definitions = new LinkedList<ActorDefinition>();
            definitions.AddLast(actorDef);

            ActorDefinition current = actorDef;
            while (current.Parent != null)
            {
                ActorDefinition? parent = m_archiveCollection.Definitions.Decorate[actorDef.Parent];
                if (parent == null)
                {
                    Log.Warn("Cannot find entity definition for parent class '{0}'", actorDef.Parent);
                    return false;
                }

                definitions.AddFirst(parent);
                current = parent;

                if (definitions.Count > RecursiveDefinitionOverflow)
                {
                    Log.Warn("Infinite recursive parent cycle detected, possible offender: {0}", actorDef.Parent);
                    return false;
                }
            }
            
            return true;
        }

        private EntityDefinition? ComposeNewDefinition(CIString name)
        {
            ActorDefinition? actorDefinition = m_archiveCollection.Definitions.Decorate[name];
            if (actorDefinition == null)
            {
                Log.Warn("Unable to create entity '{0}', definition does not exist", name);
                return null;
            }
            
            // We build it up where the front of the list corresponds to the
            // base definition, and each one after that is a child of the
            // previous until we reach the bottom level. This means if we have
            // A inherits from B inherits from C, the list is: [C, B, A]
            if (!CreateInheritanceOrderedList(actorDefinition, out LinkedList<ActorDefinition> definitions))
            {
                Log.Warn("Unable to create entity '{0}' due to errors with the actor or parents", name);
                return null;
            }
            
            int id = m_indexTracker.Next();
            EntityDefinition definition = new EntityDefinition(id, actorDefinition.Name);

            definitions.ForEach(actorDef => ApplyActorComponents(definition, actorDef));
            
            // TODO: Check if well formed after everything was added.

            // TODO: Handle 'replaces'.

            return definition;
        }
    }
}