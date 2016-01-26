namespace TrumpLanguage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Tagging;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ITaggerProvider))]
    [ContentType("trump!")]
    [TagType(typeof(TrumpTokenTag))]
    internal sealed class TrumpTokenTagProvider : ITaggerProvider
    {

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new TrumpTokenTagger(buffer) as ITagger<T>;
        }
    }

    public class TrumpTokenTag : ITag 
    {
        public TrumpTokenTypes type { get; private set; }

        public TrumpTokenTag(TrumpTokenTypes type)
        {
            this.type = type;
        }
    }

    internal sealed class TrumpTokenTagger : ITagger<TrumpTokenTag>
    {

        ITextBuffer _buffer;
        IDictionary<string, TrumpTokenTypes> _trumpTypes;

        internal TrumpTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _trumpTypes = new Dictionary<string, TrumpTokenTypes>();
            _trumpTypes["trump!"] = TrumpTokenTypes.TrumpExclaimation;
            _trumpTypes["trump."] = TrumpTokenTypes.TrumpPeriod;
            _trumpTypes["trump?"] = TrumpTokenTypes.TrumpQuestion;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<TrumpTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                int curLoc = containingLine.Start.Position;
                string[] tokens = containingLine.GetText().ToLower().Split(' ');

                foreach (string trumpToken in tokens)
                {
                    if (_trumpTypes.ContainsKey(trumpToken))
                    {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc, trumpToken.Length));
                        if( tokenSpan.IntersectsWith(curSpan) ) 
                            yield return new TagSpan<TrumpTokenTag>(tokenSpan, 
                                                                  new TrumpTokenTag(_trumpTypes[trumpToken]));
                    }

                    //add an extra char location because of the space
                    curLoc += trumpToken.Length + 1;
                }
            }
            
        }
    }
}
