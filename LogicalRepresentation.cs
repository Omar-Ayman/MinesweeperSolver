using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineSweeperSolver
{
    public class Situation
    {
        public List<Node> nodes;

        public Situation(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        public bool Eval()
        {
            return false;
        }
    }

    public class Node
    {
        public List<Senario> senarios;
        public HashSet<Atom> atoms;

        public Node(List<Senario> senarios)
        {
            this.senarios = senarios;

            atoms = new HashSet<Atom>();
            foreach (Senario sen in senarios)
                atoms = new HashSet<Atom>(atoms.Union(sen.atoms));
        }
    }

    public class Senario : ICloneable
    {
        public List<Atom> atoms;

        public Senario(List<Atom> atoms)
        {
            this.atoms = atoms;
        }

        public object Clone()
        {
            return new Senario(new List<Atom>(atoms));
        }
    }

    public class Atom
    {
        public string name;
        public bool value;

        public Atom(string name, bool value)
        {
            this.name = name;
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is Atom && name == ((Atom)obj).name && !(value ^ ((Atom)obj).value);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
