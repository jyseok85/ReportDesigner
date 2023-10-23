using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportDesigner.Blazor.Common._삭제하기_아까운_로직
{
    /// <summary>
    /// HTML의 테두리문제로 인하여 Boxshadow를 사용할 생각을 했지만
    /// 기존 모니터 (100ppi) 이하에서는 1픽셀로 잘 나오지 않는 문제로 적용못함.
    /// 28인치 4k (160) 정도에서는 정상적으로 보임.
    /// 2023년 현재 일반적인 데스크탑 환경이라면 적용하기에 무리가 있음. 대부분의 데스크탑이 4K모니터를 기본으로 쓸때에 사용하면 아주 쾌적하게 적용가능할거라고 생각됨.
    /// 아니면 모바일이나 태블릿 전용이던가.
    /// </summary>
    internal class _MakeBoxShadow
    {
        private string MakeBoxShoadow(float thickness, string color)
        {
            return MakeBoxShadow(thickness, thickness, thickness, thickness, color, color, color, color);
        }
        /// <summary>
        /// 컨트롤의 box-shadow css를 만든다.
        /// </summary>
        /// <param name="top">top 테두리 두께</param>
        /// <param name="right">right 테두리 두께</param>
        /// <param name="bottom">bottom 테두리 두께</param>
        /// <param name="left">left 테두리 두께</param>
        private string MakeBoxShadow(float top, float right, float bottom, float left, string topColor, string rightColor, string bottomColor, string leftColor)
        {
            string leftBoxShadow = $"-{left * 0.5f}px 0 0 0 {leftColor} inset ,   {left * 0.5f}px 0 0 0 {leftColor}";
            string rightBoxShadow = $"{right * 0.5f}px 0 0 0 {rightColor} inset, -{right * 0.5f}px 0 0 0 {rightColor}";
            string topBoxShadow = $"0 {top * 0.5f}px 0 0 {topColor} inset  ,    0 -{top * 0.5f}px 0 0 {topColor}";
            string bottomBoxShadow = $"0 -{bottom * 0.5f}px 0 0 {bottomColor} inset  , 0 -{bottom * 0.5f}px 0 0 {bottomColor}";

            //좌측상단 모서리
            string leftTopVertex = $"-{left * 0.5f}px  -{top * 0.5f}px    0 0 {topColor}";
            string rightTopVertex = $"{right * 0.5f}px  -{top * 0.5f}px    0 0 {topColor}";
            string leftBottomVertex = $"-{left * 0.5f}px  {bottom * 0.5f}px    0 0 {bottomColor}";
            string rightBottomVertex = $"{right * 0.5f}px  {bottom * 0.5f}px    0 0 {bottomColor}";

            string boxShadowCSS = $"box-shadow:{leftBoxShadow},{rightBoxShadow},{topBoxShadow},{bottomBoxShadow}";

            //2면이 있을경우에 꼭지점을 위한 CSS를 추가한다..
            //Outline에 각 테두리별 속성이 없어서 이게 먼짓인지..
            if (IsDraw(top, left, topColor, leftColor))
                boxShadowCSS += $",{leftTopVertex}";
            if (IsDraw(top, right, topColor, rightColor))
                boxShadowCSS += $",{rightTopVertex}";
            if (IsDraw(bottom, left, bottomColor, leftColor))
                boxShadowCSS += $",{leftBottomVertex}";
            if (IsDraw(bottom, right, bottomColor, rightColor))
                boxShadowCSS += $",{rightBottomVertex}";

            boxShadowCSS += ";";

            return boxShadowCSS;
            bool IsDraw(float thickness1, float thickness2, string color1, string color2)
            {
                if (thickness1 > 0 && color1 != "transparent" && thickness2 > 0 && color2 != "transparent")
                    return true;
                else
                    return false;
            }
        }
    }
}
