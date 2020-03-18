using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using ReactiveUI;
using Paradox.Common;
using StellarisModManager.Views;

namespace StellarisModManager.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        public ModManager Manager { get; }
        private readonly MainWindow _window;
        private readonly ListBox _modList;
        private ListBoxItem _dragItem;
        private int _selectedIndex = -1;

        public int SelectedIndex
        {
            get => this._selectedIndex;
            set => this.RaiseAndSetIfChanged(ref this._selectedIndex, value);
        }

        public MainWindowViewModel(MainWindow window)
        {
            this._window = window;
            this._modList = this._window.Find<ListBox>("ModList");
            this.Manager = new ModManager();

            var moveUp = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x > 0);
            var moveDown = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x >= 0 && x < this.Manager.Enabled.Count - 1);

            this.Save = ReactiveCommand.Create(() => this.Manager.Save());
            this.AlphaSort = ReactiveCommand.Create(() => this.Manager.AlphaSort());
            this.ReverseOrder = ReactiveCommand.Create(() => this.Manager.ReverseOrder());
            this.MoveToTop = ReactiveCommand.Create(() => this.Manager.MoveToTop(this.SelectedIndex), moveUp.Select(x => x));
            this.MoveUp = ReactiveCommand.Create(() => this.Manager.MoveUp(this.SelectedIndex), moveUp.Select(x => x));
            this.MoveDown = ReactiveCommand.Create(() => this.Manager.MoveDown(this.SelectedIndex), moveDown.Select(x => x));
            this.MoveToBottom = ReactiveCommand.Create(() => this.Manager.MoveToBottom(this.SelectedIndex), moveDown.Select(x => x));
            this.CheckAll = ReactiveCommand.Create(() => this.Manager.CheckAll());
            this.UncheckAll = ReactiveCommand.Create(() => this.Manager.UncheckAll());
            this.InvertCheck = ReactiveCommand.Create(() => this.Manager.InvertCheck());
        }

        public void StartDrag(object sender, PointerPressedEventArgs e) =>
            this._dragItem = this._modList.GetLogicalChildren().Cast<ListBoxItem>().Single(x => x.IsPointerOver);

        public void DoDrag(object sender, PointerEventArgs e)
        {
            if (this._dragItem == null) return;
            var list = this._modList.GetLogicalChildren().ToList();

            var hoveredItem = (ListBoxItem)list.FirstOrDefault(x => this._window.GetVisualsAt(e.GetPosition(this._window)).Contains(((IVisual)x).GetVisualChildren().First()));
            var dragItemIndex = list.IndexOf(this._dragItem);
            var hoveredItemIndex = list.IndexOf(hoveredItem);

            this.ClearDropStyling();
            if (!Equals(hoveredItem, this._dragItem)) hoveredItem?.Classes.Add(dragItemIndex > hoveredItemIndex ? "BlackTop" : "BlackBottom");
        }

        public void EndDrag(object sender, PointerReleasedEventArgs e)
        {
            var hoveredItem = (ListBoxItem)this._modList.GetLogicalChildren().FirstOrDefault(x => this._window.GetVisualsAt(e.GetPosition(this._window)).Contains(((IVisual)x).GetVisualChildren().First()));
            if (this._dragItem != null && hoveredItem != null && !Equals(this._dragItem, hoveredItem))
            {
                var a = this._dragItem.DataContext as ModEntry;
                var b = hoveredItem.DataContext as ModEntry;
                this.Manager.Enabled.Move(this.Manager.Enabled.IndexOf(a),
                    this.Manager.Enabled.IndexOf(b));
                this.Manager.Validate();

            }

            this.ClearDropStyling();
            this._dragItem = null;
        }

        private void ClearDropStyling()
        {
            foreach (var item in this._modList.GetLogicalChildren().Cast<ListBoxItem>())
            {
                item.Classes.RemoveAll(new[] { "BlackTop", "BlackBottom" });
            }
        }

        public IReactiveCommand Save { get; }
        public IReactiveCommand AlphaSort { get; }
        public IReactiveCommand ReverseOrder { get; }
        public IReactiveCommand MoveToTop { get; }
        public IReactiveCommand MoveUp { get; }
        public IReactiveCommand MoveDown { get; }
        public IReactiveCommand MoveToBottom { get; }
        public IReactiveCommand CheckAll { get; }
        public IReactiveCommand UncheckAll { get; }
        public IReactiveCommand InvertCheck { get; }
    }
}
