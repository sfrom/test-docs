using Acies.Docs.Services.HandlebarHelpers;
using HandlebarsDotNet;
using System;
using Xunit;

namespace Acies.Docs.Services.Tests.Unit
{
    public class HandlebarInjectionHelperTests
    {
        public HandlebarInjectionHelperTests()
        {
            HandlebarInjectionHelper.RegisterHelpers();
        }

        [Fact]
        public void MathMultiply_ReturnsMultipliedValue_WhenBothValuesDefined()
        {
            // Arrange

            string helper = "math-multiply";
            dynamic data = new { one = 2.00, two = 2.50 };
            string html = $"{{{{{{{helper} one two}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("5", result);
        }

        [Fact]
        public void MathMultiply_ReturnsMinusOne_WhenInvalidValues()
        {
            // Arrange

            string helper = "math-multiply";
            dynamic data = new { one = 2.00, two = 2.50 };
            string html = $"{{{{{{{helper} one nonexistentnumber}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("-1", result);
        }

        [Fact]
        public void MathMod_ReturnsModulusValue_WhenBothValuesDefined()
        {
            // Arrange

            string helper = "math-mod";
            dynamic data = new { one = 50.00, two = 8.00 };
            string html = $"{{{{{{{helper} one two}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("2", result);
        }

        [Fact]
        public void MathMod_ReturnsMinusOne_WhenInvalidValues()
        {
            // Arrange

            string helper = "math-mod";
            dynamic data = new { one = 50.00, two = 8.00 };
            string html = $"{{{{{{{helper} one nonexistentnumber}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("-1", result);
        }

        [Fact]
        public void FormatNumber_ReturnsFormattedNumber_WhenFormatIsValid()
        {
            // Arrange

            string helper = "format-number";
            dynamic data = new { one = 50.5 };
            string html = $"{{{{{{{helper} one 5 'da-DK'}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("50,50000", result);
        }

        [Fact]
        public void FormatNumber_ReturnsFormattedNumberRoundedDecimals_WhenFormatIsValid()
        {
            // Arrange

            string helper = "format-number";
            dynamic data = new { one = -50.543215432154321 };
            string html = $"{{{{{{{helper} one 5 'da-DK'}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("-50,54322", result);
        }

        [Fact]
        public void FormatNumber_ReturnsMinusOne_WhenInvalidValues()
        {
            // Arrange

            string helper = "format-number";
            dynamic data = new { one = 50.5 };
            string html = $"{{{{{{{helper} one 2 'XX-YY'}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("-1", result);
        }

        [Fact]
        public void FormatDate_ReturnsFormattedDate_WhenFormatIsValid()
        {
            // Arrange

            string helper = "format-date";
            dynamic data = new { one = new DateTime(2000, 10, 5), two = "MM/dd/yyyy" };
            string html = $"{{{{{{{helper} one two}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("10/05/2000", result);
        }

        [Fact]
        public void FormatDate_ReturnsMinusOne_WhenInvalidValues()
        {
            // Arrange

            string helper = "format-date";
            dynamic data = new { one = new DateTime(2000, 10, 5), two = "MM/dd/yyyy" };
            string html = $"{{{{{{{helper} nonexistentdate two}}}}}}";

            // Act

            string result = Handlebars.Compile(html)(data);

            // Assert

            Assert.Equal("-1", result);
        }

        //[Fact]
        //public void InjectLocalImage_ReturnsBase64Image_WhenImageFound()
        //{
        //
        //}

        //[Fact]
        //public void InjectLocalImage_ReturnsBlank_WhenInvalidValues()
        //{
        //
        //}

        //[Fact]
        //public void InjectLocalImage_ReturnsFullImagePath_WhenImageNotFound()
        //{
        //
        //}

        //[Fact]
        //public void InjectLocalSvg_ReturnsSvgContent_WhenSvgFound()
        //{
        //
        //}

        //[Fact]
        //public void InjectLocalSvg_ReturnsFullImagePath_WhenSvgNotFound()
        //{
        //
        //}

        //[Fact]
        //public void InjectLocalSvg_ReturnsBlank_WhenInvalidValues()
        //{
        //
        //}
    }
}