import React from "react";
import { FormFeedback, FormGroup, Input, Label } from "reactstrap";
import InputGroupWrapper from "./InputGroupWrapper";
import { FormFieldProps } from "./formTypes";

const FormField = <T extends Record<string, any>>({
  register,
  error,
  type = "text",
  name,
  label,
  inputPrepend,
  inputAppend,
  required = false,
  maxLength,
  readOnly = false,
  autoComplete,
  children,
  ...options
}: FormFieldProps<T>) => {
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
      <InputGroupWrapper prepend={inputPrepend} append={inputAppend}>
        <Input
          innerRef={ref}
          id={`field-${name}`}
          name={autoComplete ? name : `field-${name}`} // less likely to autofill
          type={type}
          invalid={!!error}
          readOnly={readOnly}
          autoComplete={autoComplete ?? "new-password"}
          {...rest}
        >
          {children}
        </Input>
      </InputGroupWrapper>
      {!!error && <FormFeedback>{error.message}</FormFeedback>}
    </FormGroup>
  );
};

export default FormField;
