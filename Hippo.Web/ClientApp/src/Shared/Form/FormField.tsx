import React from "react";
import { FormFeedback, FormGroup, Input, Label } from "reactstrap";
import InputGroupWrapper from "./InputGroupWrapper";
import { FormFieldFullProps } from "./formTypes";

const FormField = <T extends Record<string, any>>({
  register,
  error,
  type = "text",
  name,
  label,
  prepend,
  append,
  required = false,
  maxLength,
  readOnly = false,
  ...options
}: FormFieldFullProps<T>) => {
  const { ref, ...rest } = register(name, {
    required: {
      value: required,
      message: `${label} is required`,
    },
    maxLength: {
      value: maxLength,
      message: `${label} must be less than ${maxLength} characters`,
    },
    ...options,
  });

  return (
    <FormGroup>
      <Label for={`field-${name}`}>{label}</Label>
      <InputGroupWrapper prepend={prepend} append={append}>
        <Input
          innerRef={ref}
          id={`field-${name}`}
          type={type}
          invalid={!!error}
          readOnly={readOnly}
          {...rest}
        />
      </InputGroupWrapper>
      {!!error && <FormFeedback>{error}</FormFeedback>}
    </FormGroup>
  );
};

export default FormField;
