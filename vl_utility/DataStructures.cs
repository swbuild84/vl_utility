//**************************************
// Name: C# 2.0 Graph Class
// Description:Stores vertices and edges along with user-defined data, and enables a number of queries on the resulting graph
// By: Chris Forbes
//
//
// Inputs:-Types associated with vertices and edges

//
// Returns:None
//
//Assumes:None
//
//Side Effects:None
//This code is copyrighted and has limited warranties.
//Please see http://www.Planet-Source-Code.com/xq/ASP/txtCodeId.4587/lngWId.10/qx/vb/scripts/ShowCode.htm
//for details.
//**************************************

using System;
using System.Collections.Generic;
using System.Text;
namespace ChrisForbes.DataStructures
{
    #region Edge
    /// <summary>
    /// Represents additional data which is attached to an edge.
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    /// <typeparam name="E">The edge type</typeparam>
    public struct Edge<T, E>
        where T : IEquatable<T>
    {
        private E data;
        private T start;
        private T end;
        /// <summary>
        /// The attached data
        /// </summary>
        public E Data
        {
            get { return data; }
            set { data = value; }
        }
        /// <summary>
        /// The start node
        /// </summary>
        public T Start
        {
            get { return start; }
        }
        /// <summary>
        /// The end node
        /// </summary>
        public T End
        {
            get { return end; }
        }
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Edge<T, E> edge = (Edge<T, E>)obj;
            return (edge.start.Equals(start) && edge.end.Equals(end));
        }
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return start.GetHashCode() ^ end.GetHashCode();
        }
        /// <summary>
        /// Creates a new instance of the Edge structure
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <param name="data">The attached data</param>
        internal Edge(T start, T end, E data)
        {
            this.start = start;
            this.end = end;
            this.data = data;
        }
    }
    #endregion
    /// <summary>
    /// Implements a graph where the nodes are of type T, and the edges are described by an object of type E
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    /// <typeparam name="E">The edge data type</typeparam>
    public class Graph<T, E> : ICollection<T>
        where T : IEquatable<T>
    {
        List<T> nodes = new List<T>();
        List<Edge<T, E>> edges = new List<Edge<T, E>>();
        readonly bool isDirected;
        readonly bool allowsReflexivity;
        /// <summary>
        /// Returns whether the graph is directed
        /// </summary>
        public bool IsDirected
        {
            get
            {
                return isDirected;
            }
        }
        /// <summary>
        /// Returns whether the graph allows reflexivity (nodes connected to themselves)
        /// </summary>
        public bool AllowsReflexivity
        {
            get
            {
                return allowsReflexivity;
            }
        }
        /// <summary>
        /// Returns the set of nodes in the graph
        /// </summary>
        public IEnumerable<T> Nodes
        {
            get
            {
                foreach (T node in nodes)
                    yield return node;
            }
        }
        /// <summary>
        /// Returns the set of edges in the graph
        /// </summary>
        public IEnumerable<Edge<T, E>> Edges
        {
            get
            {
                foreach (Edge<T, E> e in edges)
                    yield return e;
            }
        }
        /// <summary>
        /// Adds a node to the graph
        /// </summary>
        /// <param name="node">A node</param>
        public void Add(T node)
        {
            if (nodes.Contains(node)) throw new ArgumentException("The specified node is already in the graph");
            nodes.Add(node);
        }
        /// <summary>
        /// Adds an edge between two existing nodes in the graph. Throws ArgumentException
        /// if the edge already exists, either node does not exist in the graph, of if the edgeis illegal.
        /// </summary>
        /// <param name="start">The start node for the edge</param>
        /// <param name="end">The end node for the edge</param>
        /// <param name="data">Additional data to attach to the edge</param>
        public void AddEdge(T start, T end, E data)
        {
            if (!allowsReflexivity && start.Equals(end))
                throw new ArgumentException("Reflexivity is not allowed");
            if (!nodes.Contains(start))
                throw new ArgumentException("The start node is not in the graph");
            if (!nodes.Contains(end))
                throw new ArgumentException("The end node is not in the graph");
            if (ContainsEdge(start, end))
                throw new ArgumentException("The edge is already in the graph");
            edges.Add(new Edge<T, E>(start, end, data));
        }
        /// <summary>
        /// Determines whether the graph contains an edge starting at start, and ending at end
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <returns></returns>
        public bool ContainsEdge(T start, T end)
        {
            if (isDirected)
            {
                foreach (Edge<T, E> p in edges)
                {
                    if (!p.Start.Equals(start))
                        continue;
                    if (!p.End.Equals(end))
                        continue;
                    return true;
                }
                return false;
            }
            else
            {
                foreach (Edge<T, E> p in edges)
                {
                    if (!p.Start.Equals(start) && !p.Start.Equals(end))
                        continue;
                    if (!p.End.Equals(start) && !p.End.Equals(end))
                        continue;
                    if (start.Equals(end))
                        return true;
                    if (!p.Start.Equals(p.End))
                        return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Removes the edge from start to end, if it exists. If an edge is removed, returns true.
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <returns>True iff an edge was removed</returns>
        public bool RemoveEdge(T start, T end)
        {
            for (int edge = edges.Count - 1; edge >= 0; edge--)
            {
                if (edges[edge].Start.Equals(start) && edges[edge].End.Equals(end))
                {
                    edges.RemoveAt(edge);
                    return true;
                }
                if (!isDirected && edges[edge].End.Equals(start) &&
edges[edge].Start.Equals(end))
                {
                    edges.RemoveAt(edge);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns the set of edges from the given node (ie that have it as their start)
        /// </summary>
        /// <param name="start">The start node</param>
        /// <returns>The set of edges beginning at start</returns>
        public IEnumerable<Edge<T, E>> GetEdgesFrom(T start)
        {
            foreach (Edge<T, E> e in edges)
            {
                if (e.Start.Equals(start))
                {
                    yield return e;
                    continue;
                }
                if (!isDirected && e.End.Equals(start))
                {
                    yield return e;
                    continue;
                }
            }
        }
        /// <summary>
        /// Returns the set of edges ending at the given node
        /// </summary>
        /// <param name="end">The ending node</param>
        /// <returns>The set of edges ending at the given node</returns>
        public IEnumerable<Edge<T, E>> GetEdgesTo(T end)
        {
            foreach (Edge<T, E> e in edges)
            {
                if (e.End.Equals(end))
                {
                    yield return e;
                    continue;
                }
                if (!isDirected && e.Start.Equals(end))
                {
                    yield return e;
                    continue;
                }
            }
        }
        /// <summary>
        /// Returns the set of edges ending at the given node
        /// </summary>
        /// <param name="end">The ending node</param>
        /// <returns>The set of edges ending at the given node</returns>
        public Edge<T, E> GetEdge(T start, T end)
        {
            foreach (Edge<T, E> e in edges)
            {
                if (e.Start.Equals(start) && e.End.Equals(end))
                {
                    return e;
                }
            }
            throw new ArgumentException("The edge is not in the graph");
        }
        /// <summary>
        /// Returns the degree of the specified node; IE the number of edges connected to it.
        /// </summary>
        /// <param name="node">The node to find the degree of</param>
        /// <returns>The degree of the node</returns>
        public int GetDegree(T node)
        {
            int degree = 0;
            foreach (Edge<T, E> e in GetEdgesTo(node))
                degree++;
            if (isDirected)
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                    degree++;
            return degree;
        }
        /// <summary>
        /// Returns the set of nodes directly connected to the given node, where the edge starts
        /// at the given node.
        /// </summary>
        /// <param name="node">A node</param>
        /// <returns>The set of nodes</returns>
        public IEnumerable<T> GetNodesFrom(T node)
        {
            if (isDirected)
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                    yield return e.End;
            else
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                {
                    if (e.Start.Equals(e.End))
                    {
                        yield return node;
                        continue;
                    }
                    if (e.Start.Equals(node))
                        yield return e.End;
                    if (e.End.Equals(node))
                        yield return e.Start;
                }
        }
        /// <summary>
        /// Returns the set of nodes directly connected to the given node, where the edge ends
        /// at the given node.
        /// </summary>
        /// <param name="node">A node</param>
        /// <returns>The set of nodes</returns>
        public IEnumerable<T> GetNodesTo(T node)
        {
            if (isDirected)
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                    yield return e.Start;
            else
                foreach (Edge<T, E> e in GetEdgesFrom(node))
                {
                    if (e.Start.Equals(e.End))
                    {
                        yield return node;
                        continue;
                    }
                    if (e.Start.Equals(node))
                        yield return e.End;
                    if (e.End.Equals(node))
                        yield return e.Start;
                }
        }
        #region ICollection<T> Members
        /// <summary>
        /// Removes all edges and nodes
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
        }
        /// <summary>
        /// Determines if the graph contains the specified node
        /// </summary>
        /// <param name="node">The node to look for</param>
        /// <returns>True if the node is in the graph</returns>
        public bool Contains(T node)
        {
            return nodes.Contains(node);
        }
        /// <summary>
        /// Copies the nodes to an array. Not supported.
        /// </summary>
        /// <param name="array">An array to copy to</param>
        /// <param name="arrayIndex">The array index to begin at</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException("CopyTo(T[],int) is not supported on Graph<T,E>");
        }
        /// <summary>
        /// Returns the number of nodes in the graph
        /// </summary>
        public int Count
        {
            get { return nodes.Count; }
        }
        /// <summary>
        /// Returns whether the graph is readonly
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }
        /// <summary>
        /// Removes the specified node from the graph, along with all edges connected to it.
        /// </summary>
        /// <param name="node">The node to remove</param>
        /// <returns>True iff the node is removed</returns>
        public bool Remove(T node)
        {
            if (!nodes.Remove(node))
                return false;
            for (int edge = edges.Count - 1; edge >= 0; edge--)
            {
                if (edges[edge].Start.Equals(node) || edges[edge].End.Equals(node))
                    edges.RemoveAt(edge);
            }
            return true;
        }
        #endregion
        #region IEnumerable<T> Members
        /// <summary>
        /// Returns an enumerator over the nodes.
        /// </summary>
        /// <returns>An enumerator of nodes</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
        #endregion
        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator
        /// </summary>
        /// <returns>An enumerator of nodes</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
        #endregion
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        /// <param name="directed">Whether the graph should be directed</param>
        /// <param name="allowsReflexivity">Whether the graph should allow reflexivity</param>
        public Graph(bool directed, bool allowsReflexivity)
        {
            this.isDirected = directed;
            this.allowsReflexivity = allowsReflexivity;
        }
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        /// <param name="directed">Whether the graph should be directed</param>
        public Graph(bool directed)
            : this(directed, true)
        { }
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        public Graph()
            : this(true, true)
        { }
    }
    /// <summary>
    /// Implements a graph with nodes of type T, and no attached edge data.
    /// If you need to attach data to edges, use Graph&lt;T,E&gt; instead.
    /// </summary>
    /// <typeparam name="T">The node type</typeparam>
    public class Graph<T> : Graph<T, object>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        /// <param name="directed">Whether the graph should be directed</param>
        /// <param name="allowsReflexivity">Whether the graph should allow reflexivity</param>
        public Graph(bool directed, bool allowsReflexivity)
            : base(directed, allowsReflexivity)
        { }
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        /// <param name="directed">Whether the graph should be directed</param>
        public Graph(bool directed)
            : base(directed)
        { }
        /// <summary>
        /// Constructs a new, empty graph.
        /// </summary>
        public Graph()
            : base()
        { }
        /// <summary>
        /// Adds an edge between start and end. Throws ArgumentException if the edge is invalid.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void AddEdge(T start, T end)
        {
            base.AddEdge(start, end, null);
        }
    }
}

