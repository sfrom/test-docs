using Acies.Docs.Models;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using Xunit;

namespace Acies.Docs.Services.Tests.Unit
{
    public class ValidateServiceTests
    {
        [Fact]
        public void ValidateInput_ShouldReturnError_WhenNoDataButValidationPropertiesExist()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = "{}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': false, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 1}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.True(!string.IsNullOrWhiteSpace(response.ErrorMessage));
            Assert.Contains("missing from object: name", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShoudReturnSuccess_WhenNoDataAndValidationProperties()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = "{}";
            var templateInput = new TemplateInput() { Validation = "{}"};

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenDataButNoValidationPropertiesExist()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test'}";
            var templateInput = new TemplateInput() { Validation = "{}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenInputDataPropertyFoundInValidationProperties()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': false, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 1}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenInputDataPropertyMissingInRequiredValidationProperties()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': false, 'required': ['name', 'name2'], 'properties': {'name': {'type': 'string', 'minLength': 1}, 'name2': {'type': 'string', 'minLength': 1}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.True(!string.IsNullOrWhiteSpace(response.ErrorMessage));
            Assert.Contains("missing from object: name2", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenInputDataHasMorePropertiesThanInValidationPropertiesAndNoAdditionalPropsAllowed()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': false, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 1}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.True(!string.IsNullOrWhiteSpace(response.ErrorMessage));
            Assert.Contains("schema does not allow additional properties", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenInputDataHasMorePropertiesThanInValidationPropertiesAndAdditionalPropsAllowed()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 1}}}" };


            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenRegExValidatedOnProperty()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'pattern': '^[a-zA-Z_]*$', 'minLength': 1}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenRegExValidatedOnPropertyWithIllegalChars()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test1234', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'pattern': '^[a-zA-Z_]*$', 'minLength': 1}}}" };


            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("does not match regex pattern", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenStringPropertyInsideMaxLength()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'maxLength': 4}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenStringPropertyOutsideMaxLength()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'maxLength': 3}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("exceeds maximum length of 3", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenStringPropertyOverMinLength()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 4}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenStringPropertyBelowMinLength()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 5}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("less than minimum length of 5", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenStringPropertyInsideMinAndMaxLength()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 3, 'maxLength': 5}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenStringPropertyOverMinAndOverMaxLength()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'testing', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 3, 'maxLength': 5}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("exceeds maximum length of 5", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenStringPropertyUnderMinAndUnderMaxLength()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'value', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['name'], 'properties': {'name': {'type': 'string', 'minLength': 5, 'maxLength': 10}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("less than minimum length of 5", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenPropertyIsInteger()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 1, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'maximum': 5}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenPropertyIsNotIntegerAndInputTypeIsInteger()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'testing', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'maximum': 5}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);


            //Assert
            Assert.False(response.Success);
            Assert.Contains("Expected Integer but got String", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenIntergerPropertyBelowMax()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 1, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'maximum': 5}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenIntegerPropertyAboveMax()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 10, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'maximum': 5}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("Integer 10 exceeds maximum value of 5", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenIntergerPropertyAboveMin()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 5, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'minimum': 3}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenIntegerPropertyBelowMin()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 1, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'minimum': 3}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("Integer 1 is less than minimum value of 3", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenIntegerPropertyAboveMinAndUnderMax()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 5, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'minimum': 3, 'maximum': 5}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenIntegerPropertyAboveMinAndAboveMax()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 3, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'minimum': 1, 'maximum': 2}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("Integer 3 exceeds maximum value of 2", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenIntegerPropertyUnderMinAndUnderMax()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'testing', test: 1, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'integer', 'minimum': 2, 'maximum': 3}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("Integer 1 is less than minimum value of 2", response.ErrorMessage);
        }





        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenPropertyIsDecimal()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 1.0, test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'number', 'minimum': 1, 'maximum': 3}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenPropertyIsNotDecimalAndInputTypeIsDecimal()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'testing', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'number', 'minimum': 1, 'maximum': 3}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains("Expected Number but got String", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnSuccess_WhenPropertyIsStringUri()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'https://dr.dk', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'string', 'format': 'uri'}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.True(response.Success);
            //Assert.Contains("Expected Number but got String", response.ErrorMessage);
        }

        [Fact]
        public void ValidateInput_ShouldReturnError_WhenPropertyIsInvalidStringUri()
        {
            //Arrange
            var validationService = new ValidationService();
            var data = @"{name: 'test', test: 'dr.dk', test2: 'value2'}";
            var templateInput = new TemplateInput() { Validation = @"{'$schema': 'http://json-schema.org/draft-07/schema', 'type': 'object', 'additionalProperties': true, 'required': ['test'], 'properties': {'test': {'type': 'string', 'format': 'uri'}}}" };

            //Act
            var response = validationService.ValidateJson(data, templateInput.Validation);

            //Assert
            Assert.False(response.Success);
            Assert.Contains(@"String 'dr.dk' does not validate against format 'uri'", response.ErrorMessage);
        }

    }
}
