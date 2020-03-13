using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using DynamicData;
using ReactiveUI;
using StellarisModManager.Core;
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
        private readonly List<ListBoxItem> _dragItems;
        private int _firstDragItem = -1;
        private int _lastDragItem = -1;

        public int SelectedIndex
        {
            get => this._selectedIndex;
            set => this.RaiseAndSetIfChanged(ref this._selectedIndex, value);
        }

        public MainWindowViewModel(MainWindow window)
        {
            this._dragItems = new List<ListBoxItem>();
            this._window = window;
            this._modList = this._window.Find<ListBox>("ModList");
            this.Manager = new ModManager();
            this.Manager.Load();

            var moveUp = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x > 0);
            var moveDown = this.WhenAnyValue(x => x.SelectedIndex).Select(x => x >= 0 && x < this.Manager.Mods.Count - 1);

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

        public void StartDrag(object sender, PointerPressedEventArgs e)
        {
            this._dragItem = this._modList.GetLogicalChildren().Cast<ListBoxItem>().Single(x => x.IsPointerOver);
            this._dragItems.Clear();
            var list = this._modList.GetLogicalChildren().ToList();
            this._dragItems.AddRange(list.Cast<ListBoxItem>().Where(x => x.IsSelected));
            this._firstDragItem = this._dragItems.Select(x => list.IndexOf(x)).Min(x => x);
            this._lastDragItem = this._dragItems.Select(x => list.IndexOf(x)).Max(x => x);
        }

        public void DoDrag(object sender, PointerEventArgs e)
        {
            //if (this._dragItems.Count == 0) return;
            if (this._dragItem == null) return;

            var hoveredItem = (ListBoxItem)this._modList.GetLogicalChildren().FirstOrDefault(x => this._window.GetVisualsAt(e.GetPosition(this._window)).Contains(((IVisual)x).GetVisualChildren().First()));
            var dragItemIndex = this._modList.GetLogicalChildren().ToList().IndexOf(this._dragItem);
            var hoveredItemIndex = this._modList.GetLogicalChildren().ToList().IndexOf(hoveredItem);

            this.ClearDropStyling();
            /*if (!this._dragItems.Contains(hoveredItem))
            {
                if(hoveredItemIndex > this._lastDragItem) hoveredItem?.Classes.Add("BlackTop");
                else if (hoveredItemIndex < this._firstDragItem) hoveredItem?.Classes.Add("BlackBottom");
            }*/
            if (!Equals(hoveredItem, this._dragItem)) hoveredItem?.Classes.Add(dragItemIndex > hoveredItemIndex ? "BlackTop" : "BlackBottom");
        }

        public void EndDrag(object sender, PointerReleasedEventArgs e)
        {
            var hoveredItem = (ListBoxItem)this._modList.GetLogicalChildren().FirstOrDefault(x => this._window.GetVisualsAt(e.GetPosition(this._window)).Contains(((IVisual)x).GetVisualChildren().First()));
            if (this._dragItem != null && hoveredItem != null && !Equals(this._dragItem, hoveredItem))
            {
                this.Manager.Mods.Move(
                    this._modList.GetLogicalChildren().ToList().IndexOf(this._dragItem),
                    this._modList.GetLogicalChildren().ToList().IndexOf(hoveredItem));
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
