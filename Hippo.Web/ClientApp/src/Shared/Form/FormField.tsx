import React from "react";
import { RegisterOptions, UseFormRegister, FieldPath } from "react-hook-form";
import { FormFeedback, FormGroup, Input, Label } from "reactstrap";
import { InputType } from "reactstrap/types/lib/Input";
import InputGroupWrapper from "./InputGroupWrapper";

export type FormFieldProps<T> = {
  type?: InputType;
  name: FieldPath<T>;
  label: string;
  prepend?: React.ReactNode;
  append?: React.ReactNode;
  required?: boolean; // overwriting RegisterOptions required
  maxLength?: number;
  readOnly?: boolean;
};

type FormFieldFullProps<T> = RegisterOptions &
  FormFieldProps<T> & {
    register: UseFormRegister<T>;
    error: string;
  };

const FormField = <T extends Record<string, any>>({
  type = "text",
  name,
  label,
  prepend,
  append,
  register,
  error,
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
