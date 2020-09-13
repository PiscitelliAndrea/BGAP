using Microsoft.AspNetCore.Components;
using BGAP.web.Client.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BGAP.web.Client.Pages
{
    public partial class FifteenPuzzleGame : ComponentBase
    {
        #region Injections

        [Inject]
        protected TilesGenerator Tiles { get; set; }

        #endregion

        #region Internal properties

        protected Timer timer { get; set; }
        protected bool TimerStarted = false;

        protected List<Tile> tiles = new List<Tile>();
        protected Tile tile = new Tile();
        protected int NumOfRows = 2;
        protected TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(0);

        #endregion

        #region Life Cycle events

        protected override void OnInitialized()
        {
            tiles = Tiles.GenerateTiles(NumOfRows);
        }

        #endregion

        #region Tiles Methods

        protected void ClickTile(int riga, int colonna)
        {
            if (!Tiles.Done)
            {
                if (!TimerStarted)
                {
                    TimerStarted = true;
                    StartCounter();

                    this.StateHasChanged();
                }

                tiles = Tiles.TryMoveTile(riga, colonna);

                if (Tiles.Done)
                    StopCounter();

                this.StateHasChanged();
            }
        }

        protected void Restart()
        {
            tiles = Tiles.Restart();

            ResetCounter();
            TimerStarted = true;
            StartCounter();
            
            this.StateHasChanged();
        }

        #endregion

        #region Timer Methods

        void StartCounter()
        {
            timer = new Timer(new TimerCallback(_ =>
            {
                ElapsedTime = ElapsedTime.Add(new TimeSpan(0, 0, 1));

                // Note that the following line is necessary to refresh the UI
                InvokeAsync(() =>
                {
                    StateHasChanged();
                });
            }), null, 1000, 1000);
        }

        void ResetCounter()
        {
            ElapsedTime = new TimeSpan(0, 0, 0);
        }

        void StopCounter()
        {
            timer.Dispose();
        }

        #endregion
    }
}