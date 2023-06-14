using MongoDB.Driver;

namespace Demo.Common.DBBase.Repos.Base
{
    public static class MongoCollectionQueryByPageExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <param name="filterDefinition"></param>
        /// <param name="sortDefinition"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// 
        //[Obsolete("To slow query")]
        public static async Task<(int totalPages, long count, IReadOnlyList<TDocument> data)> AggregateByPage<TDocument>(
            this IMongoCollection<TDocument> collection,
            FilterDefinition<TDocument> filterDefinition,
            SortDefinition<TDocument> sortDefinition,
            ProjectionDefinition<TDocument, TDocument> projectionDef,
            int page,
            int pageSize)
        {
            var countFacet = AggregateFacet.Create("count",
                PipelineDefinition<TDocument, AggregateCountResult>.Create(new[]
                {
                PipelineStageDefinitionBuilder.Count<TDocument>()
                }));

            var agg = new EmptyPipelineDefinition<TDocument>()
                .AppendStage(PipelineStageDefinitionBuilder.Skip<TDocument>((page - 1) * pageSize))
                .AppendStage(PipelineStageDefinitionBuilder.Limit<TDocument>(pageSize));

            var defs = new[]
                {
                PipelineStageDefinitionBuilder.Skip<TDocument>((page - 1) * pageSize),
                PipelineStageDefinitionBuilder.Limit<TDocument>(pageSize),
                };
            if (sortDefinition != null)
            {
                agg.AppendStage(PipelineStageDefinitionBuilder.Sort(sortDefinition));
                //defs = new[]
                //{
                //PipelineStageDefinitionBuilder.Sort(sortDefinition),
                //PipelineStageDefinitionBuilder.Skip<TDocument>((page - 1) * pageSize),
                //PipelineStageDefinitionBuilder.Limit<TDocument>(pageSize),
                //};
            }

            if (projectionDef != null)
            {
                agg.AppendStage(PipelineStageDefinitionBuilder.Project<TDocument, TDocument>(projectionDef));
            }

            var dataFacet = AggregateFacet.Create("data", agg);

            var aggregateOptions = new AggregateOptions { };
            if (sortDefinition != null)
            {
                aggregateOptions = new AggregateOptions { AllowDiskUse = true };
            }
            var aggregation = await collection.Aggregate(aggregateOptions)
                .Match(filterDefinition)
                .Facet(countFacet, dataFacet)
                .ToListAsync();

            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count;
            if (count is null)
            {
                count = 0;
            }

            var totalPages = (int)Math.Ceiling((double)count / pageSize);

            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<TDocument>();

            return (totalPages, count.Value, data);
        }
    }
}
