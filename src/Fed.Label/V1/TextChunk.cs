namespace Fed.Label.V1
{
    /// <summary>
    /// Represents a "chunk" of text on a label
    /// </summary>
    class TextChunk
    {
        public string Text { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public int FontStyle { get; set; }
        public bool EmbedFont { get; set; }
        public bool SameLine { get; set; }
    }
}