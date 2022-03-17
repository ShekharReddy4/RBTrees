using System;
using System.Collections.Generic;

namespace RedBlackTrees
{
    public class Program
    {
        static void Traverse<T>(TreeNode<T> node, int rec, int recblack, int depth)
        {
            if (node == null)
            {
                if (depth != recblack)
                    throw new ArgumentException(
                        $"ERROR : expected count of {depth} but node has {recblack}");
                return;
            }

            foreach (var next in new[] { node.Left, node.Right })
                Traverse(next, rec + 1,
                    node.Color == RbColor.Black ? recblack + 1 : recblack, depth);


        }

        static void Main(string[] args)
        {
            var tree = new RBTree<int>();
            var list = new List<int>();
            var rand = new Random(1);
            int N = 1000000;
            int M = N / 10;
            int i;
            for (i = 0; i < N; i++)
                list.Add(rand.Next(0, 50 * N));

            int cnt = 0;
            int pos1 = 0, pos2 = 0;
            for (i = 0; i < M; i++)
            {
                tree.Add(list[pos1++]);
                if (cnt++ % M == 0)
                    Traverse(tree.Root, 0, 0, tree.Depth);
            }
            for (int j = 0; j < 9; j++)
            {
                for (i = 0; i < M; i++)
                {
                    tree.Add(list[pos1++]);
                    if (cnt++ % M == 0)
                        Traverse(tree.Root, 0, 0, tree.Depth);
                }
                for (i = 0; i < M; i++)
                {
                    tree.Remove(list[pos2++]);
                    if (cnt++ % M == 0)
                        Traverse(tree.Root, 0, 0, tree.Depth);
                }
            }


            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

        }
    }
}
