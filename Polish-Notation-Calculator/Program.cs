using System;
using System.Collections.Generic;
using System.Linq;

namespace Polish_Notation_Calculator
{
    class Program
    {
        private static Stack<string> stack = new Stack<string>();
        private static Dictionary<string, double> vars = new Dictionary<string, double>();
        private static bool exception = false;
        static void Main(string[] args)
        {
            string line = Console.ReadLine();
            while (line.Length != 0)
            {
                var words = from s in line.Split(' ') where s.Length != 0 select s;
                foreach (string w in words)
                {
                    try
                    {
                        double res = 0, l = 0, r = 0;
                        switch (w)
                        {
                            case "=":
                                PopValue(out double v);
                                string key = stack.Pop();
                                vars[key] = v;
                                stack.Push(v.ToString());
                                break;
                            case "+":
                                PopValues(out r, out l);
                                res = l + r;
                                stack.Push(res.ToString());
                                break;
                            case "-":
                                PopValues(out r, out l);
                                res = l - r;
                                stack.Push(res.ToString());
                                break;
                            case "*":
                                PopValues(out r, out l);
                                res = l * r;
                                stack.Push(res.ToString());
                                break;
                            case "/":
                                PopValues(out r, out l);
                                res = l / r;
                                stack.Push(res.ToString());
                                break;
                            case "%":
                                PopValues(out r, out l);
                                res = l % r;
                                stack.Push(res.ToString());
                                break;
                            case "pow":
                                PopValues(out r, out l);
                                res = Math.Pow(l, r);
                                stack.Push(res.ToString());
                                break;
                            case "root":
                                PopValues(out r, out l);
                                res = Math.Pow(l, 1 / r);
                                stack.Push(res.ToString());
                                break;
                            default:
                                stack.Push(w);
                                break;
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex.Message + "\nPress any key to continue");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.ReadKey(true);
                        Console.Clear();
                        exception = true;
                        break;
                    }
                }
                if (!exception)
                {
                    Console.WriteLine(stack.Pop());
                }
                exception = false;
                line = Console.ReadLine();
            }
            Console.Clear();
            Console.WriteLine("Press any key to exit the app");
            Console.ReadKey(true);
        }
        static void PopValues(out double r, out double l)
        {
            string right = stack.Pop();
            if (!double.TryParse(right, out r) && !vars.TryGetValue(right, out r))
            {
                throw new InvalidOperationException($"Variable {right} doesn't exist");
            }
            string left = stack.Pop();
            if (!double.TryParse(left, out l) && !vars.TryGetValue(left, out l))
            {
                throw new InvalidOperationException($"Variable {left} doesn't exist");
            }
        }
        static void PopValue(out double v)
        {
            string value = stack.Pop();
            if (!double.TryParse(value, out v) && !vars.TryGetValue(value, out v))
            {
                throw new InvalidOperationException($"Variable {value} doesn't exist");
            }
        }
    }
}
