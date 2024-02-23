using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common.Data.EtcComponents
{
    /// <summary>
    /// 폰트모델에 넣기 애매한 , 텍스트 속성들이 포함된다.
    /// </summary>
    public class Paragraph : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        /// <summary>
        /// 수직 정렬
        /// </summary>
        public string VerticalAlignment { get; set; } = "center";

        /// <summary>
        /// 수평 정렬
        /// </summary>
        public string HorizontalAlignment { get; set; } = "center";

        /// <summary>
        /// [고급] 줄 간격(LineSpacing)
        /// </summary>
        public double LineHeight { get; set; } = 1;

        /// <summary>
        /// [고급] 텍스트 방향
        /// </summary>
        public TextDirection TextDirection { get; set; } = TextDirection.Horizontal;

        /// <summary>
        /// [고급] 텍스트가 컨트롤 사이즈를 넘어갔을 경우 자동 개행 여부
        /// </summary>
        public bool WordWrap { get; set; } = false;

        /// <summary>
        /// [고급] 문자 간격
        /// </summary>
        public double CharacterSpacing { get; set; } = 0;


        /// <summary>
        /// [고급] enter 키 입력시 개행 여부
        /// </summary>
        public bool MultiLine { get; set; } = true;

        /// <summary>
        /// [고급] 텍스트 자동조절 여부, 그러나 늘어나는 기능은 없다.
        /// </summary>
        public bool AutoFitText { get; set; } = false;


        /// <summary>
        /// [고급] 최소배율 텍스트가 짤리더라도 이 값 이하로 줄어들지 않음
        /// </summary>
        public int MinScale { get; set; } = 20;

        /// <summary>
        /// [고급] 현재 표시할 배율
        /// </summary>
        public int CurrentScale { get; set; } = 100;

        /// <summary>
        /// [고급] 최대배율
        /// </summary>
        public int MaxScale { get; set; } = 100;


        /// <summary>
        /// [고급] 텍스트가 컨트롤 사이즈를 넘어갔을 경우 처리방법
        /// DR 기본값은 보이기, 그러나 테이블에서는 기본값이 클립이다.
        /// </summary>
        public OverFlowText OverFlowText { get; set; } = OverFlowText.Visible;

    }
}
