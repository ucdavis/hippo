import React from "react";
import { FieldErrors, RegisterOptions, UseFormRegister } from "react-hook-form";
import { FormFeedback, FormGroup, Input, Label } from "reactstrap";
import { OrderModel } from "../../types";
import { InputType } from "reactstrap/types/lib/Input";
import InputGroupWrapper from "./InputGroupWrapper";

export type FormFieldProps<T> = {
  type?: InputType;
  name: keyof T;
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
    errors: FieldErrors<T>;
  };

// if you want to use this component for other models, add type next to OrderModel
const FormField: React.FC<FormFieldFullProps<OrderModel>> = ({
  type = "text",
  name,
  label,
  prepend,
  append,
  register,
  errors,
  required = false,
  maxLength,
  readOnly = false,
  ...options
}) => {
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
          invalid={!!errors[name]}
          readOnly={readOnly}
          {...rest}
        />
      </InputGroupWrapper>
      {errors[name] && <FormFeedback>{errors[name].message}</FormFeedback>}
    </FormGroup>
  );
};

export default FormField;
