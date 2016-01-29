using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace TableParser
{
    public class TableRenderer<T>
    {
        private readonly int _frequency = 1000;
        private readonly int _x;
        private readonly int _y;
        private IEnumerable<T> _content;
        private bool _update;

        public TableRenderer(string[] columnHeaders, params Func<T, object>[] valueSelectors) : this(columnHeaders, valueSelectors, null, 0, 0, 1000)
        {
        }

        public TableRenderer(string[] columnHeaders = null, Func<T, object>[] valueSelectors = null, IEnumerable<T> initialContent = null, int x = 0, int y = 0, int frequency = 1000)
        {
            _x = x;
            _y = y;
            _frequency = frequency;
            Content = initialContent;
            ColumnHeaders = columnHeaders;
            ValueSelectors = valueSelectors;

            if (ColumnHeaders == null) ColumnHeaders = DefaultColumnHeaders;
            if (ValueSelectors == null) ValueSelectors = CreateAccessorsCompiled().ToArray();
            Render();
            if (frequency > 0)
            {
                Refresh();
            }
        }

        private string[] DefaultColumnHeaders
        {
            get
            {
                var x = typeof (T);
                var lst = new List<string>();
                foreach (var p in x.GetProperties())
                {
                    if (p.CanRead) lst.Add(p.Name);
                }
                return lst.ToArray();
            }
        }

        public IEnumerable<T> Content
        {
            get { return _content; }
            set
            {
                _content = value;
                _update = true;
            }
        }

        public string[] ColumnHeaders { get; set; }
        public Func<T, object>[] ValueSelectors { get; set; }
        public Expression<Func<T, object>>[] ValueSelectorsExpressions { get; set; }

        private List<Func<T, object>> CreateAccessorsCompiled()
        {
            var accessors = new List<Func<T, object>>();
            var t = typeof (T);
            foreach (var prop in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.CanRead)
                {
                    var lambdaParam = Expression.Parameter(t, "instance");
                    Expression bodyExpression;
                    var memberAccessExpression = Expression.MakeMemberAccess(Expression.Convert(lambdaParam, t), prop);
                    if (prop.PropertyType == typeof (object))
                    {
                        // Create lambda expression:  (instance) => ((T)instance).Member
                        bodyExpression = memberAccessExpression;
                    }
                    else
                    {
                        // Create lambda expression:  (instance) => (object)((T)instance).Member
                        bodyExpression = Expression.Convert(memberAccessExpression, typeof (object));
                    }
                    var lambda = Expression.Lambda<Func<T, object>>(bodyExpression, lambdaParam);
                    accessors.Add(lambda.Compile());
                }
            }
            return accessors;
        }

        private void Render()
        {
            if (Content != null)
            {
                var c = "";
                if (ColumnHeaders != null && ValueSelectors != null)
                {
                    c = Content.ToStringTable(ColumnHeaders, ValueSelectors);
                }
                else if (ValueSelectorsExpressions != null)
                {
                    c = Content.ToStringTable(ValueSelectorsExpressions);
                }
                else
                {
                    c = Content.ToArray().ToStringTable();
                }

                if (!string.IsNullOrEmpty(c)) Render(c);
            }
        }

        private void Refresh()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(_frequency);
                    Render();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void Render(string content)
        {
            Console.Clear();
            _update = false;
            Console.SetCursorPosition(_x, _y);
            Console.WriteLine(content);
        }
    }
}