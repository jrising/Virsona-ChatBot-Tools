/******************************************************************\
 *      Class Name:     SalienceTree
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *
 *      Modifications:
 *      -----------------------------------------------------------
 *      Date            Author          Modification
 *
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.FastSerializer;

namespace DataTemple.Codeland.SearchTree
{

    public class SalienceTree<DataType> : RedBlackTree<double, DataType>, ISalienceSet<DataType>
    {
        protected double salienceSum;
        protected double smartWeight;

        public Random randgen;

        // Deserialization constructor
        public SalienceTree()
            : base()
        {
            salienceSum = 0;
            smartWeight = .25;
            randgen = new Random();
        }

        public override void Clear()
        {
            base.Clear();
            salienceSum = 0;
        }

        public double SalienceTotal
        {
            get
            {
                return salienceSum;
            }
        }

        // don't call remove-- derived classes will remove!
        public virtual void ChangeSalience(DataType obj, double before, double after)
        {
            if (obj == null)
                throw (new RedBlackException("ChangeSalience object is null"));

            // find node
            RedBlackNode<double, DataType> node = Search(before, obj);
            if (node == null)
                throw (new RedBlackException("ChangeSalience object not found"));

            Delete(node);

            Add(after, obj);
        }

        public DataType SelectSalientItem(RandomSearchQuality quality)
        {
            if (quality == RandomSearchQuality.Fast)
                return SelectSmartItem();
            else
                return SelectExactItem();
        }

        public DataType SelectRandomItem(RandomSearchQuality quality)
        {
            if (quality == RandomSearchQuality.Fast)
            {
                double position;
                return SelectWeightedItem(.5, out position).Data;
            }
            else
            {
                int chosen = randgen.Next(Count);
                RedBlackEnumerator<double, DataType> enumerator = GetNodeEnumerator();
                while (enumerator.HasMoreElements())
                {
                    RedBlackNode<double, DataType> node = enumerator.NextElement();
                    if (chosen-- == 0)
                        return node.Data;
                }

                return default(DataType);
            }
        }

        public DataType SelectSmartItem()
        {
            double position;
            RedBlackNode<double, DataType> result = SelectWeightedItem(smartWeight, out position);

            // Adjust the weight!
            // We expect an object at position to give the following
            double expected = position * 2 * (salienceSum / intCount);
            if (expected > result.Key)
            {
                // Smaller than we expected-- decrease weight!
                smartWeight = (smartWeight + .25) / 2;
            }
            else
            {
                // More even than we expected-- increase weight!
                smartWeight = (smartWeight + 0.5) / 2;
            }

            return result.Data;
        }

        public DataType SelectExpectedSalientItem()
        {
            double position;
            return SelectWeightedItem(.25, out position).Data;
        }

        public RedBlackNode<double, DataType> SelectWeightedItem(double weight, out double position) {
            if (IsEmpty())
            {
                position = .5;
                return null;
            }

            double selector = randgen.NextDouble();

            // Otherwise, we have a proper tree!
            double left = 0;    // left possible-position
            double right = 1;   // right possible-position
            RedBlackNode<double, DataType> node = rbTree;   // current node
            double expectBelow = salienceSum;   // total salience below node

            // Scale down tree
            while (node.Left != null && node.Right != null)
            {
                // should we take this node?
                double fraction = node.Key / expectBelow;
                if (selector < fraction) {
                    position = (left + right) / 2;
                    return node;
                }
                // do we recurse on left?
                if (selector < fraction + weight) {
                    node = node.Left;           // update node
                    selector -= fraction;       // adjust selector from 0 to 1
                    selector /= weight;
                    right = (left + right) / 2; // adjust possible range
                    expectBelow *= weight;      // change expected below
                } else {
                    node = node.Right;                  // update node
                    selector -= fraction + weight;      // adjust salience from 0 to 1
                    selector /= 1 - (fraction + weight);
                    left = (left + right) / 2;          // adjust possible range
                    expectBelow *= (1 - weight);        // change expected below
                    fraction = (weight + .5) / 2;       // diff between left and write is less
                }
            }

            if (node.Left == null && node.Right == null)
            {
                // this is a leaf-- take it!
                position = (left + right) / 2;
                return node;
            } else {
                // choose between these two nodes
                RedBlackNode<double, DataType> other = (node.Left == null ? node.Right : node.Left);
                double nodesSum = node.Key + other.Key;
                if (selector * nodesSum < node.Key)
                {
                    position = (2*left + right) / 3;
                    return node;
                }
                else
                {
                    position = (left + 2*right) / 3;
                    return other;
                }
            }
        }

        // Select a codelet by carefully respecting salience
        public DataType SelectExactItem() {
            double salienceToGet = randgen.NextDouble() * salienceSum;
            double salienceSoFar = 0;

            RedBlackEnumerator<double, DataType> enumerator = GetNodeEnumerator();
            while (enumerator.HasMoreElements()) {
                RedBlackNode<double, DataType> node = enumerator.NextElement();
                salienceSoFar += node.Key;
                if (salienceSoFar >= salienceToGet)
                    return node.Data;
            }

            // Salience calculation failure!
            Console.WriteLine("WARNING: Salience miscalculation!  Max is " + salienceSoFar.ToString() + " not " + salienceSum.ToString());
            salienceSum = salienceSoFar;
            // Try again!
            return SelectExactItem();
        }

        public override void Add(double key, DataType data)
        {
            base.Add(key, data);
            salienceSum += key;
        }

        // Usually don't use this, because there may be duplicates!
        public override bool Remove(double key)
        {
            if (base.Remove(key))
            {
                salienceSum -= key;
                return true;
            }
            else
                return false;
        }

        // We may have multiple, so check against object
        public override bool Remove(double key, DataType obj)
        {
            if (base.Remove(key, obj))
            {
                salienceSum -= key;
                return true;
            }
            else
                return false;
        }

        public override bool TestIntegrity()
        {
            // count up salienceSum again, and compare to old value
            double oldSalienceSum = salienceSum;
            salienceSum = 0;
            bool result = base.TestIntegrity();
            if (!result)
                return false;

            if (Math.Abs(salienceSum - oldSalienceSum) > .001)
            {
                return false;
            }

            return true;
        }

        public override int TestIntegrityNode(RedBlackNode<double, DataType> node)
        {
            int result = base.TestIntegrityNode(node);
            if (result < 0)
                return result;

            if (node != null)
                salienceSum += node.Key;
            return result;
        }

        #region IFastSerializable Members

        public override void Deserialize(SerializationReader reader)
        {
            salienceSum = reader.ReadDouble();  // salienceSum = info.GetDouble("salienceSum");
            // : base(info, ctxt)
            intCount = reader.ReadInt32();
            if (intCount != 0)
            {
                RedBlackNode<double, DataType>[] containers = new RedBlackNode<double, DataType>[intCount];
                int ii = 0;
                rbTree = DeserializeTree(reader, containers, ref ii);
                for (int jj = 0; jj < intCount; jj++)
                    containers[jj].Data = (DataType) reader.ReadPointer();
            }
            else
                rbTree = null;
        }

        public RedBlackNode<double, DataType> DeserializeTree(SerializationReader reader, RedBlackNode<double, DataType>[] containers, ref int ii)
        {
            double key = reader.ReadDouble();
            if (key == 0.0)
                return null;
            else
            {
                RedBlackNode<double, DataType> node = new RedBlackNode<double, DataType>();
                node.Key = key;
                node.Color = reader.ReadBoolean() ? RED : BLACK;
                containers[ii++] = node;

                RedBlackNode<double, DataType> left = DeserializeTree(reader, containers, ref ii);
                RedBlackNode<double, DataType> right = DeserializeTree(reader, containers, ref ii);
                if (left != null)
                    left.Parent = node;
                if (right != null)
                    right.Parent = node;
                node.Left = left;
                node.Right = right;

                return node;
            }
        }

        public override void Serialize(SerializationWriter writer)
        {
            writer.Write(salienceSum);  // info.AddValue("salienceSum", salienceSum);
            // base.GetObjectData(info, context);
            writer.Write(intCount);
            if (intCount != 0)
            {
                DataType[] elements = new DataType[intCount];
                int ii = 0;
                SerializeTree(writer, elements, ref ii, rbTree);
                for (int jj = 0; jj < intCount; jj++)
                    writer.WritePointer(elements[jj]);
            }
        }

        public void SerializeTree(SerializationWriter writer, DataType[] elements, ref int ii, RedBlackNode<double, DataType> node)
        {
            if (node == null)
                writer.Write(0.0);
            else
            {
                writer.Write(node.Key);
                writer.Write(node.Color == RED);
                elements[ii++] = node.Data;
                SerializeTree(writer, elements, ref ii, node.Left);
                SerializeTree(writer, elements, ref ii, node.Right);
            }
        }

        #endregion
    }
}
