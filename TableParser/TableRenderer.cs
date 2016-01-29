using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public TableRenderer(string[] columnHeaders, Func<T, object>[] valueSelectors, IEnumerable<T> initialContent = null, int x = 0, int y = 0, int frequency = 1000)
        {
            _x = x;
            _y = y;
            _frequency = frequency;
            Content = initialContent;
            ColumnHeaders = columnHeaders;
            ValueSelectors = valueSelectors;
            Render();
            if (frequency > 0)
            {
                Refresh();
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