using Repka.Diagnostics;

namespace Repka.Graphs
{
    public static class GraphReporting
    {
        public static Report ToReport(this IEnumerable<GraphTrace> traces) => new()
        {
            Text = "Traces",
            Records = traces.Select(trace => trace.ToReport()).ToList(),
        };

        public static Report ToReport(this GraphTrace trace) => new()
        {
            Text = $"{trace.Source?.Key} -> {trace.Target?.Key}",
            Records = trace.Select(link => link.ToReport()).ToList()
        };

        public static Report ToReport(this GraphLink link) => new()
        {
            Text = link.SourceKey,
            Records = new List<Report>
            {
                new Report { Text = string.Join(", ", link.Labels) },
                new Report { Text = link.TargetKey },
            }
        };

        //{
        //    Report report = new();
        //    Dictionary<GraphKey, Report> records = new();

        //    GraphKey? sourceKey = default;
        //    GraphKey? targetKey = default;
        //    Dictionary<GraphKey, HashSet<GraphLink>> inputs = new();
        //    Dictionary<GraphKey, HashSet<GraphLink>> outputs = new();

        //    foreach (GraphTrace trace in traces)
        //    {
        //        if (sourceKey is null)
        //            sourceKey = trace.Source?.Key;
        //        if (targetKey is null)
        //            targetKey = trace.Target?.Key;

        //        foreach (GraphLink link in trace)
        //        {
        //            records.TryAdd(link.SourceKey, new());
        //            records.TryAdd(link.TargetKey, new());

        //            if (!inputs.ContainsKey(link.TargetKey))
        //                inputs[link.TargetKey] = new();
        //            inputs[link.TargetKey].Add(link);

        //            if (!outputs.ContainsKey(link.SourceKey))
        //                outputs[link.SourceKey] = new();
        //            outputs[link.SourceKey].Add(link);
        //        }
        //    }

        //    foreach ((GraphKey key, Report record) in records)
        //    {
        //        record.Text = key;
        //        record.Records = new List<Report>
        //        {
        //            new Report
        //            {
        //                Text = "Inputs",
        //                Records = inputs[key]
        //                    .Select(link => new Report { Text = link.ToString() })
        //                    .ToList()
        //            },
        //            new Report
        //            {
        //                Text = "Outputs",
        //                Records = outputs[key]
        //                    .Select(link => new Report { Text = link.ToString() })
        //                    .ToList()
        //            }
        //        };
        //    }

        //    return report;
        //}
    }
}
