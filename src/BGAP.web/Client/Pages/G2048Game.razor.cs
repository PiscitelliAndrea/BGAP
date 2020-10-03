using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using BGAP.web.Client.Core;
using System.Threading.Tasks;

namespace BGAP.web.Client.Pages
{
    public partial class G2048Game : ComponentBase
    {
        #region Injection

        [Inject]
        protected NumbersManager Numbers { get; set; }

        #endregion

        #region Internal properties

        protected Timer Timer { get; set; }
        protected bool TimerStarted = false;

        protected List<NumberTile> numbers = new List<NumberTile>();
        protected NumberTile number;

        //private int ElapsedTime { get; set; }
        protected TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(0);

        #endregion

        #region Life Cycle events

        protected override void OnInitialized() => numbers = Numbers.GenerateTwoInitialNumbers();

        #endregion

        #region Numbers Methods

        protected async Task ClickNumber(Direction direzione)
        {
            if (!Numbers.GameOver)
            {
                if (!TimerStarted)
                {
                    TimerStarted = true;
                    StartCounter();
                }

                numbers = Numbers.TryMoveNumber(direzione);
                this.StateHasChanged();
                await Task.Delay(400);

                GetNewNumber();

                if (Numbers.GameOver)
                    StopCounter();
            }
        }

        protected void GetNewNumber()
        {
            if (Numbers.Moved)
                numbers = Numbers.GenerateNewNumber();
            this.StateHasChanged();
        }

        protected void Restart()
        {
            ResetCounter();
            TimerStarted = true;
            StartCounter();
            numbers = Numbers.Restart();
            this.StateHasChanged();
        }

        #endregion

        #region Timer Methods

        void StartCounter()
        {
            Timer = new Timer(new TimerCallback(_ =>
            {
                //ElapsedTime++;
                ElapsedTime = ElapsedTime.Add(new TimeSpan(0, 0, 1));

                // Note that the following line is necessary because otherwise
                // Blazor would not recognize the state change and not refresh the UI
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
            Timer.Dispose();
        }

        #endregion
    }
}
