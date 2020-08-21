using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Diagnostics;

namespace BellmanFord {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Directed weighted graph:");
            SingleSourceShortestPathForDirectedWeightedGraph();
            Console.WriteLine();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            Console.WriteLine();

            Console.WriteLine("Directed acyclic weighted graph:");
            SingleSourceShortestPathForDirectedAcyclicWeightedGraph();
            Console.WriteLine();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void SingleSourceShortestPathForDirectedWeightedGraph() {
            Console.WriteLine("------------------");
            Console.WriteLine("Order is different!");
            Console.WriteLine("------------------");
            Console.WriteLine();

            var verticies = new[] { "s", "y", "t", "x", "z" };
            (string source, string target, int weight)[] edges = new[] {
                ("s", "t", 6),
                ("s", "y", 7),
                ("t", "x", 5),
                ("t", "y", 8),
                ("t", "z", -4),
                ("y", "x", -3),
                ("y", "z", 9),
                ("x", "t", -2),
                ("z", "s", 2),
                ("z", "x", 7)
            };

            var g = new DirectedWeightedGraph();

            foreach (var v in verticies) _ = g.AddVertex(v);
            foreach (var e in edges) _ = g.AddEdge(g.GetVertex(e.source), g.GetVertex(e.target), e.weight);

            // start
            var result = BellmanFord(g, g.GetVertex("s"));

            Console.WriteLine();
            Console.WriteLine($"Overall result: {result}.");
        }

        private static void SingleSourceShortestPathForDirectedAcyclicWeightedGraph() {
            var verticies = new[] { "z", "y", "x", "t", "s", "r" };
            (string source, string target, int weight)[] edges = new[] {
                ("y", "z", -2),
                ("x", "z", 1),
                ("x", "y", -1),
                ("t", "x", 7),
                ("t", "y", 4),
                ("t", "z", 2),
                ("s", "t", 2),
                ("s", "x", 6),
                ("r", "s", 5),
                ("r", "t", 3)
            };

            var g = new DirectedAcyclicWeightedGraph();

            foreach (var v in verticies) _ = g.AddVertex(v);
            foreach (var e in edges) _ = g.AddEdge(g.GetVertex(e.source), g.GetVertex(e.target), e.weight);

            var topoSorted = g.PerformTopologicalSort();
            InitializeSingleSource(g, g.GetVertex("r"));
            foreach (var u in topoSorted) {
                foreach (var v in g.GetAdjacent(u)) {
                    var edge = g.GetEdge(u, v);

                    if (ShouldBeRelaxed(edge))
                        Relax(edge);
                }
            }

            PrintGraph(g, "Result:");
        }

        static void PrintGraph(DirectedWeightedGraph g, string caption) {
            Console.WriteLine(caption);
            Console.Write("  V  ");
            Console.Write("|");
            Console.Write("  D  ");
            Console.Write("|");
            Console.Write("  P  ");
            Console.WriteLine();

            foreach (var v in g.Vertices) {
                Console.Write($"  {v.Id}  ");
                Console.Write("|");
                Console.Write($"  {v.Distance}  ");
                Console.Write("|");

                string p = v.Predecessor is null ? "-" : v.Predecessor?.Id;
                Console.Write($"  {p}  ");
                Console.WriteLine();
            }

            Console.WriteLine("-------------------");
        }

        static void InitializeSingleSource(DirectedWeightedGraph g, Vertex source) {
            foreach (var v in g.Vertices) {
                v.Distance = double.PositiveInfinity;
                v.Predecessor = null;
            }

            source.Distance = 0;
        }

        static void Relax(Edge e) {
            var u = e.Source;
            var v = e.Target;
            var w = e.Weight;

            v.Distance = u.Distance + w;
            v.Predecessor = u;
        }

        static bool ShouldBeRelaxed(Edge e) {
            var u = e.Source;
            var v = e.Target;
            var w = e.Weight;

            return v.Distance > u.Distance + w;
        }

        static bool BellmanFord(DirectedWeightedGraph g, Vertex source) {
            InitializeSingleSource(g, source);
            PrintGraph(g, "// iss");

            for (int i = 1, count = g.Vertices.Count() - 1; i < count; i++) {
                foreach (var e in g.Edges) {
                    if (ShouldBeRelaxed(e)) {
                        Relax(e);

                        PrintGraph(g, $"{e.Source.Id} -> {e.Target.Id}");
                    }
                }
            }

            var flag = true;
            foreach (var e in g.Edges) {
                var u = e.Source;
                var v = e.Target;
                var w = e.Weight;

                if (v.Distance > u.Distance + w) {
                    flag = false;

                    Console.WriteLine($"False for: [{e.Id}] edge.");
                }

            }

            return flag;
        }
    }

    [DebuggerDisplay("{Id}")]
    public class Vertex {
        public Vertex(string id) {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException($"{nameof(id)} is null or empty.", nameof(id));

            Id = id;
        }

        public string Id { get; }

        public int Low { get; set; }

        public int OpenedAt { get; set; }

        public int ClosedAt { get; set; }

        public double Distance { get; set; }

        public VertexColor Color { get; set; }

        public int Weight { get; set; }

        public Vertex Predecessor { get; set; }

        public List<Edge> TreeEdges { get; } = new List<Edge>();

        public List<Edge> BackEdges { get; } = new List<Edge>();

        public List<Vertex> Children { get; } = new List<Vertex>();
    }

    [DebuggerDisplay("{Id}")]
    public class Edge {
        public Edge(Vertex source, Vertex target) {
            Source = source ?? throw new ArgumentNullException(nameof(source), $"{nameof(source)} is null.");
            Target = target ?? throw new ArgumentNullException(nameof(target), $"{nameof(target)} is null.");

            var u = source.Id;
            var w = target.Id;
            Id = $"({u}, {w})";
        }

        public string Id { get; }

        public Vertex Source { get; }

        public Vertex Target { get; }

        public int BCC { get; set; }

        public int Weight { get; set; }

        public bool IsBridge { get; set; }
    }

    public class AdjacencyList {
        protected Dictionary<string, (Vertex vertex, IEnumerable<Edge> edges)> adjacencyList;

        public AdjacencyList() { adjacencyList = new Dictionary<string, (Vertex, IEnumerable<Edge>)>(); }

        public Edge AddEdge(Vertex source, Vertex target, int weight) {
            if (!HasVertex(source)) throw new ArgumentException("Source vertex not found.", nameof(source));

            if (!HasVertex(target)) throw new ArgumentException("Target vertex not found", nameof(target));


            var edge = new Edge(source, target);
            edge.Weight = weight;
            var (v, edges) = adjacencyList[source.Id];
            adjacencyList[source.Id] = (v, Enumerable.Append(edges, edge));
            return edge;
        }

        public Vertex AddVertex(string id) {
            if (adjacencyList.TryGetValue(id, out var _)) throw new ArgumentException($"Key [{id}] already exist.",
                                                                                        nameof(id));

            var v = new Vertex(id);
            adjacencyList.Add(id, (v, Enumerable.Empty<Edge>()));
            return v;
        }

        public IEnumerable<Vertex> GetAdjacent(Vertex vertex) => GetAdjacent(vertex?.Id);

        public IEnumerable<Vertex> GetAdjacent(string id) {
            if (!adjacencyList.TryGetValue(id, out var tuple)) throw new ArgumentException($"Key [{id}] not found.",
                                                                                            nameof(id));

            var (_, edges) = tuple;
            foreach (var edge in edges) yield return edge.Target;
        }

        public bool HasVertex(Vertex v) => HasVertex(v.Id);

        public bool HasVertex(string id) => GetVertex(id) != null;

        public bool HasEdge(Edge edge) => HasEdge(edge.Source, edge.Target);

        public bool HasEdge(Vertex source, Vertex target) => GetEdge(source, target) != null;

        public Vertex GetVertex(string id) {
            if (!adjacencyList.TryGetValue(id, out var tuple))
                return null;

            return tuple.vertex;
        }

        public Edge GetEdge(Vertex source, Vertex target) {
            var sourceId = source.Id;
            if (!HasVertex(sourceId)) return null;

            var targetId = target.Id;
            var (_, edges) = adjacencyList[sourceId];

            bool predicate(Edge edge) => edge.Target.Id == targetId;
            return edges.FirstOrDefault(predicate);
        }

        public IEnumerable<Edge> Edges => adjacencyList.Values.SelectMany(x => x.edges);

        public IEnumerable<Vertex> Vertices => adjacencyList.Values.Select(x => x.vertex);
    }

    public abstract class AbstractGraph {
        protected AdjacencyList adjacencyList;

        public AbstractGraph() { adjacencyList = new AdjacencyList(); }

        public bool HasVertex(Vertex v) => adjacencyList.HasVertex(v);

        public bool HasVertex(string id) => adjacencyList.HasVertex(id);

        public bool HasEdge(Vertex source, Vertex target) => adjacencyList.HasEdge(source, target);

        public Vertex AddVertex(string id) => adjacencyList.AddVertex(id);

        public virtual Edge AddEdge(Vertex source, Vertex target, int weight) => adjacencyList.AddEdge(source, target, weight);

        public IEnumerable<Edge> Edges => adjacencyList.Edges;

        public IEnumerable<Vertex> Vertices => adjacencyList.Vertices;

        public Edge GetEdge(Vertex source, Vertex target) => adjacencyList.GetEdge(source, target);

        public Vertex GetVertex(string id) => adjacencyList.GetVertex(id);

        public IEnumerable<Vertex> GetAdjacent(Vertex vertex) => GetAdjacent(vertex?.Id);

        public IEnumerable<Vertex> GetAdjacent(string id) => adjacencyList.GetAdjacent(id);
    }

    public enum VertexColor : ushort {
        White,
        Gray,
        Black
    }


    public class DirectedWeightedGraph : AbstractGraph {
        public DirectedWeightedGraph() { }

        public void PerformDFS(Action<Vertex> onStarted = null, Action<Vertex> onFinished = null) {
            int time;

            dfs();

            void dfs() {
                foreach (var vertex in Vertices) {
                    vertex.Color = VertexColor.White;
                    vertex.Predecessor = null;
                }

                time = 0;

                foreach (var vertex in Vertices)
                    if (vertex.Color == VertexColor.White)
                        dfsVisit(vertex);
            }

            void dfsVisit(Vertex s) {
                time += 1;

                s.OpenedAt = time;
                s.Color = VertexColor.Gray;

                onStarted?.Invoke(s);

                foreach (var vertex in GetAdjacent(s)) {
                    if (vertex.Color == VertexColor.White) {
                        vertex.Predecessor = s;
                        dfsVisit(vertex);
                    }
                }

                s.Color = VertexColor.Black;
                time += 1;
                s.ClosedAt = time;

                onFinished?.Invoke(s);
            }
        }
    }

    public class DirectedAcyclicWeightedGraph : DirectedWeightedGraph {
        public DirectedAcyclicWeightedGraph() { }

        public IEnumerable<Vertex> PerformTopologicalSort() {
            var ll = new LinkedList<Vertex>();

            void handleFinished(Vertex x) => ll.AddLast(x);
            PerformDFS(onFinished: handleFinished);

            return ll.Reverse();
        }
    }

}