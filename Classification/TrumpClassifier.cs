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
    [TagType(typeof(ClassificationTag))]
    internal sealed class TrumpClassifierProvider : ITaggerProvider
    {

        [Export]
        [Name("trump!")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition TrumpContentType = null;

        [Export]
        [FileExtension(".trump")]
        [ContentType("trump!")]
        internal static FileExtensionToContentTypeDefinition TrumpFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<TrumpTokenTag> trumpTagAggregator = 
                                            aggregatorFactory.CreateTagAggregator<TrumpTokenTag>(buffer);

            return new TrumpClassifier(buffer, trumpTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }

    internal sealed class TrumpClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<TrumpTokenTag> _aggregator;
        IDictionary<TrumpTokenTypes, IClassificationType> _trumpTypes;

        internal TrumpClassifier(ITextBuffer buffer, 
                               ITagAggregator<TrumpTokenTag> trumpTagAggregator, 
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = trumpTagAggregator;
            _trumpTypes = new Dictionary<TrumpTokenTypes, IClassificationType>();
            _trumpTypes[TrumpTokenTypes.TrumpExclaimation] = typeService.GetClassificationType("trump!");
            _trumpTypes[TrumpTokenTypes.TrumpPeriod] = typeService.GetClassificationType("trump.");
            _trumpTypes[TrumpTokenTypes.TrumpQuestion] = typeService.GetClassificationType("trump?");
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            foreach (var tagSpan in this._aggregator.GetTags(spans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return 
                    new TagSpan<ClassificationTag>(tagSpans[0], 
                                                   new ClassificationTag(_trumpTypes[tagSpan.Tag.type]));
            }
        }
    }
}
