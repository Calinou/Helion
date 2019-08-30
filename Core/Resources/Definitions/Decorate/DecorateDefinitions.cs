using System.Collections.Generic;
using Helion.Resources.Archives.Entries;
using Helion.Resources.Definitions.Decorate.Parser;
using Helion.Util;
using MoreLinq;

namespace Helion.Resources.Definitions.Decorate
{
    public class DecorateDefinitions
    {
        private readonly Dictionary<CIString, ActorDefinition> m_definitions = new Dictionary<CIString, ActorDefinition>();
        private readonly Dictionary<int, ActorDefinition> m_definitionsByEditorNumber = new Dictionary<int, ActorDefinition>();

        public ActorDefinition? this[CIString name] => m_definitions.TryGetValue(name, out ActorDefinition? def) ? def : null;
        public ActorDefinition? this[int editorNum] => m_definitionsByEditorNumber.TryGetValue(editorNum, out ActorDefinition? def) ? def : null;

        public bool Contains(CIString name) => m_definitions.ContainsKey(name);

        public void AddDecorateDefinitions(Entry entry)
        {
            DecorateParser parser = new DecorateParser();
            if (parser.Parse(entry))
                parser.ActorDefinitions.ForEach(AddDefinition);
        }

        private void AddDefinition(ActorDefinition definition)
        {
            m_definitions[definition.Name] = definition;
            
            if (definition.EditorNumber != null)
                m_definitionsByEditorNumber[definition.EditorNumber.Value] = definition;
        }
    }
}