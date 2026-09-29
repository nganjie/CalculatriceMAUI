namespace CalculatriceMAUI
{
    public partial class MainPage : ContentPage
    {
        private readonly CalculatorEngine engine = new();
        private bool? isLandscape;

        public MainPage()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private void OnDigitClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Text, out int digit))
                engine.InputDigit(digit);
            UpdateDisplay();
        }

        private void OnDecimalClicked(object? sender, EventArgs e)
        {
            engine.InputDecimalPoint();
            UpdateDisplay();
        }

        private void OnOperatorClicked(object? sender, EventArgs e)
        {
            if (sender is Button button && Enum.TryParse(button.CommandParameter?.ToString(), out Operation operation))
                engine.SetOperation(operation);
            UpdateDisplay();
        }

        private void OnEqualsClicked(object? sender, EventArgs e)
        {
            engine.Evaluate();
            UpdateDisplay();
        }

        private void OnClearClicked(object? sender, EventArgs e)
        {
            engine.Clear();
            UpdateDisplay();
        }

        private void OnBackspaceClicked(object? sender, EventArgs e)
        {
            engine.Backspace();
            UpdateDisplay();
        }

        private void OnToggleSignClicked(object? sender, EventArgs e)
        {
            engine.ToggleSign();
            UpdateDisplay();
        }

        private void OnPercentClicked(object? sender, EventArgs e)
        {
            engine.Percent();
            UpdateDisplay();
        }

        private void OnSquareRootClicked(object? sender, EventArgs e)
        {
            engine.SquareRoot();
            UpdateDisplay();
        }

        private void OnSquareClicked(object? sender, EventArgs e)
        {
            engine.Square();
            UpdateDisplay();
        }

        private void OnInverseClicked(object? sender, EventArgs e)
        {
            engine.Inverse();
            UpdateDisplay();
        }

        private void OnMemoryClearClicked(object? sender, EventArgs e)
        {
            engine.MemoryClear();
            UpdateDisplay();
        }

        private void OnMemoryRecallClicked(object? sender, EventArgs e)
        {
            engine.MemoryRecall();
            UpdateDisplay();
        }

        private void OnMemoryAddClicked(object? sender, EventArgs e)
        {
            engine.MemoryAdd();
            UpdateDisplay();
        }

        private void OnMemorySubtractClicked(object? sender, EventArgs e)
        {
            engine.MemorySubtract();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            ResultLabel.Text = engine.Display;
            ResultLabel.TextColor = engine.HasError
                ? Color.FromArgb("#FF453A")
                : (Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Color.FromArgb("#1C1C1E"));

            // Un espace garde la hauteur de la ligne même quand elle est vide.
            ExpressionLabel.Text = string.IsNullOrEmpty(engine.Expression) ? " " : engine.Expression;
            MemoryIndicator.IsVisible = engine.HasMemory;

            AdjustResultFontSize();
            ScrollExpressionToEnd();
        }

        private async void ScrollExpressionToEnd()
        {
            try
            {
                if (ExpressionScroll.Width > 0)
                    await ExpressionScroll.ScrollToAsync(ExpressionLabel, ScrollToPosition.End, false);
            }
            catch (Exception)
            {
                // Le défilement n'est qu'un confort visuel.
            }
        }

        private void OnDisplaySizeChanged(object? sender, EventArgs e) => AdjustResultFontSize();

        /// <summary>
        /// Réduit la police du résultat pour que le nombre tienne toujours
        /// sur une ligne, quelle que soit la taille de l'écran.
        /// </summary>
        private void AdjustResultFontSize()
        {
            double width = DisplayBorder.Width - DisplayBorder.Padding.HorizontalThickness;
            double height = DisplayBorder.Height - DisplayBorder.Padding.VerticalThickness;
            if (width <= 0 || height <= 0)
                return;

            int length = Math.Max(ResultLabel.Text?.Length ?? 1, 1);
            double maxSize = Math.Clamp(height * 0.45, 24, 64);
            double fitSize = width / (length * 0.62);
            ResultLabel.FontSize = Math.Clamp(fitSize, 14, maxSize);
            ExpressionLabel.FontSize = Math.Clamp(maxSize * 0.38, 13, 22);
        }

        /// <summary>
        /// Réorganise la page selon l'orientation : écran au-dessus du clavier
        /// en portrait, écran à gauche et clavier à droite en paysage.
        /// </summary>
        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            if (Width <= 0 || Height <= 0)
                return;

            bool landscape = Width > Height;
            if (landscape != isLandscape)
            {
                isLandscape = landscape;
                RootGrid.RowDefinitions.Clear();
                RootGrid.ColumnDefinitions.Clear();

                if (landscape)
                {
                    RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                    RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                    RootGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1.3, GridUnitType.Star)));
                    Grid.SetRow(PadGrid, 0);
                    Grid.SetColumn(PadGrid, 1);
                }
                else
                {
                    RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(2, GridUnitType.Star)));
                    RootGrid.RowDefinitions.Add(new RowDefinition(new GridLength(5, GridUnitType.Star)));
                    RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                    Grid.SetRow(PadGrid, 1);
                    Grid.SetColumn(PadGrid, 0);
                }

                Grid.SetRow(DisplayBorder, 0);
                Grid.SetColumn(DisplayBorder, 0);
            }

            UpdateKeyFontSize(landscape);
        }

        private void UpdateKeyFontSize(bool landscape)
        {
            double usableWidth = Width - RootGrid.Padding.HorizontalThickness;
            double usableHeight = Height - RootGrid.Padding.VerticalThickness;

            double padWidth = landscape ? usableWidth * 1.3 / 2.3 : usableWidth;
            double padHeight = landscape ? usableHeight : usableHeight * 5 / 7;

            double keyWidth = padWidth / 4;
            double keyHeight = padHeight / 6;
            double keySize = Math.Min(keyWidth, keyHeight);

            Resources["KeyFontSize"] = Math.Clamp(keySize * 0.42, 14, 30);
            Resources["SmallKeyFontSize"] = Math.Clamp(keySize * 0.3, 11, 18);
        }
    }
}
