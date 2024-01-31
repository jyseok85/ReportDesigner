using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using ReportDesigner.Blazor.Common.Data.BaseClass;
using ReportDesigner.Blazor.Common.Data.Model;
using ReportDesigner.Blazor.Common.UI.ReportControls.Controls;
using System.Reflection;

namespace ReportDesigner.Blazor.Common.Services
{
    
    public class SelectedControlService
    {
        [Inject]
        public required DesignerCSSService CSS { get; set; }
        [Inject]
        public required IJSRuntime JsRuntime { get; set; }

        enum Type
        { 
            report,
            layer,
            band,
            control
        }
     
        

        private List<ReportComponentModel> models = new List<ReportComponentModel>();
        public List<ReportComponentModel> Models => models;

        //todo : 쓸지 말지 아직 모르겠음.
        private object razorComponent;
        //public object RazorComponent => razorComponent;

        /// <summary>
        /// 변경시점 - 컨트롤을 생성할때, 밴드를 클릭할때
        /// </summary>
        public BandBase? CurrentBand { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="model">선택한 오브젝트</param>
        /// <param name="razorComponent">현재 Razor 오브젝트</param>
        public void OnPointerDown(PointerEventArgs e, ReportComponentModel model, object razorComponent = null)
        {
            SelectControl(e.CtrlKey, model, razorComponent);
        }
        public void SelectControl(bool isMultiSelect, ReportComponentModel model, object parentBand = null)
        {
            BeforeSelectedModel = CurrentSelectedModel; 
            Console.WriteLine($"SelectedService - SelectControl : {CurrentSelectedModel.Name} razor {parentBand?.GetType()}");
            if (isMultiSelect == false)
            {
                models.ForEach(x => x.Selected = false);
                models.Clear();
            }
            models.Add(model);
            model.Selected = true;
            
            CurrentSelectedModel = model;

            if (parentBand is BandBase)
            {
                this.CurrentBand = parentBand as BandBase;
            }

            this.razorComponent = razorComponent;

        }
        public ReportComponentModel BeforeSelectedModel { get; set; }
        public ReportComponentModel LastSelectModel => models[models.Count - 1];
       
        public async void ApplyResize(int x, int y, int width, int height, ReportComponentModel parent)
        {
            LastSelectModel.X += x;
            LastSelectModel.Y += y;

            if (LastSelectModel.X < 0)
            {
                width += LastSelectModel.X;
                LastSelectModel.X = 0;
                LastSelectModel.AbsoluteOffsetX = parent.AbsoluteOffsetX;
            }
            if (LastSelectModel.Y < 0)
            {
                height += LastSelectModel.Y;
                LastSelectModel.Y = 0;
                LastSelectModel.AbsoluteOffsetY = parent.AbsoluteOffsetY;
            }


            //오른쪽 밴드 이후 영역으로 나가는지 체크
            if (x >= 0)
            {
                if (width + LastSelectModel.AbsoluteOffsetX > parent.Right + parent.AbsoluteOffsetX)
                {
                    int 변경하는가로사이즈 = width + LastSelectModel.AbsoluteOffsetX;
                    int 부모밴드의오른쪽좌표 = parent.Right + parent.AbsoluteOffsetX;
                    int diff = 변경하는가로사이즈 - 부모밴드의오른쪽좌표;
                    width -= diff;
                }
            }

            string msg = $"X:{LastSelectModel.X}, Width:{width}";
            Console.WriteLine(msg);

            int minimumWidth = CSS.DefaultPadding * 2;
            int minimumHeight = CSS.DefaultPadding * 2;

            
            if (LastSelectModel.Type == ReportComponentModel.Control.Table)
            {
                minimumWidth = (minimumWidth * LastSelectModel.TableInfo.ColCount) + 1;
                minimumHeight = (minimumHeight * LastSelectModel.TableInfo.RowCount) + 4;

                //var size = await JsRuntime.InvokeAsync<Dictionary<string,float>>("getDivSize", LastSelectModel.Uid);

                //minimumWidth = (int)size["width"];
                //minimumHeight = (int)size["height"];
            }

            if (minimumWidth > width)
                width = minimumWidth;

            if (minimumHeight > height)
                height = minimumHeight;

            //일반 컨트롤의 경우 모델사이즈를 변경하고, 리프레시를 해주면 반영되지만.
            //테이블의 경우 각 셀의 사이즈에 따라서 외부 Tr 의 사이즈가 변경된다..
            if (LastSelectModel.Type == ReportComponentModel.Control.Table)
            {
                LastSelectModel.TableInfo.UpdateCellSize(width, height);
                //너비 정보가 없다면 현재 생성된 컨트롤을 가지고 만들어 준다. 
                //if (LastSelectModel.TableInfo.ColWidths is null)
                //{
                //    LastSelectModel.TableInfo.ColWidths = new Dictionary<int, int>();
                //    foreach (ReportComponentModel model in LastSelectModel.Children)
                //    {
                //        int col = model.TableCellInfo.Col;
                //        if (LastSelectModel.TableInfo.ColWidths.ContainsKey(col) == false)
                //            LastSelectModel.TableInfo.ColWidths.Add(col, model.Width);
                //    }
                //}

                //if (LastSelectModel.TableInfo.RowHeights is null)
                //{
                //    LastSelectModel.TableInfo.RowHeights = new Dictionary<int, int>();
                //    foreach (ReportComponentModel model in LastSelectModel.Children)
                //    {
                //        int row = model.TableCellInfo.Row;
                //        if (LastSelectModel.TableInfo.RowHeights.ContainsKey(row) == false)
                //            LastSelectModel.TableInfo.RowHeights.Add(row, model.Height);
                //    }
                //}


                ////현재 총 사이즈를 가지고 각 개별 사이즈를 계산한다. 
                //var totalWidth = LastSelectModel.TableInfo.ColWidths.Values.Sum();

                ////todo : 처음 생성될때는 상관없지만, 불러오기나, 테두리를 변경하고나서는 이 계산을 다시 해줘야 한다. 
                //var 왼쪽테두리사이즈 = 0.5f;
                //var 오른쪽테두리사이즈 = 0.5f;
                //int 가로테두리사이즈 = (int)(왼쪽테두리사이즈 + 오른쪽테두리사이즈);

                //int lastWidth = width - 가로테두리사이즈;

                ////현재의 컨트롤 비율에 맞춰서 변경되는 사이즈로 늘리거나, 줄여준다.
                //for (int i= 0; i < LastSelectModel.TableInfo.ColWidths.Count; i++)
                //{                   
                //    //마지막 컬럼의 경우 나머지값을 넣어준다. 
                //    if (LastSelectModel.TableInfo.ColWidths.Count - 1 == i)
                //    {
                //        LastSelectModel.TableInfo.ColWidths[i] = lastWidth;
                //    }
                //    else
                //    {
                //        var colWidth = (int)Math.Round((double)LastSelectModel.TableInfo.ColWidths[i] / totalWidth * width);
                //        LastSelectModel.TableInfo.ColWidths[i] = colWidth;
                //        lastWidth -= colWidth;
                //    }
                //}

                //int totalHeight = LastSelectModel.TableInfo.RowHeights.Values.Sum();

                ////if (totalHeight == 0)
                ////{
                ////    var count = LastSelectModel.TableInfo.RowHeights.Count;
                ////    int defaultHeight = (int)(height / count);
                ////    for (int i = 0; i < count; i++)
                ////    {
                ////        LastSelectModel.TableInfo.RowHeights[i] = defaultHeight;
                ////    }
                ////    Console.WriteLine("HEIGHTS: " + string.Join(",", LastSelectModel.TableInfo.RowHeights));
                ////}
                ////else
                //{
                //    var 위쪽테두리사이즈 = 0.5f;
                //    var 아래쪽테두리사이즈 = 0.5f;
                //    int 세로테두리사이즈 = (int)(위쪽테두리사이즈 + 아래쪽테두리사이즈);

                //    int lastHeight = height - 세로테두리사이즈;

                //    //현재의 컨트롤 비율에 맞춰서 변경되는 사이즈로 늘리거나, 줄여준다.
                //    for (int i = 0; i < LastSelectModel.TableInfo.RowHeights.Count; i++)
                //    {

                //        //마지막 컬럼의 경우 나머지값을 넣어준다. 
                //        if (LastSelectModel.TableInfo.RowHeights.Count - 1 == i)
                //        {
                //            LastSelectModel.TableInfo.RowHeights[i] = lastHeight;
                //        }
                //        else
                //        {
                //            var rowHeight = (int)Math.Round((double)LastSelectModel.TableInfo.RowHeights[i] / totalHeight * height);
                //            LastSelectModel.TableInfo.RowHeights[i] = rowHeight;
                //            lastHeight -= rowHeight;
                //        }
                //    }
                //    Console.WriteLine("HEIGHTS: " + string.Join(",", LastSelectModel.TableInfo.RowHeights));
                //}

               

                //foreach (ReportComponentModel model in  LastSelectModel.Children)
                //{
                //    int col = model.TableCellInfo.Col;
                //    model.Width = LastSelectModel.TableInfo.ColWidths[col];

                //    int row = model.TableCellInfo.Row;
                //    model.Height = LastSelectModel.TableInfo.RowHeights[row] - 1;
                //}
                //태그의 사이즈로 전부 업데이트
                //LastSelectModel.Children.ForEach(x => x.TableCellInfo.RazorCellRef.GetDivSize());

                
            }


          

            LastSelectModel.Width = width;
            LastSelectModel.Height = height;
        }

        public required ReportComponentModel CurrentSelectedModel { get; set; } = new ReportComponentModel();

        public void SetEditMode()
        {
            CurrentSelectedModel.IsEditMode = true;
        }

        public ReportComponentModel? CopiedModel = new ReportComponentModel();

        public void CopyControl()
        {
            CopiedModel = CurrentSelectedModel.DeepClone();
            Console.WriteLine("Control Copied");

        }
        
    }
}
