import React from "react";
import { FieldErrors, RegisterOptions, UseFormRegister } from "react-hook-form";
import { FormFeedback, FormGroup, Input, Label } from "reactstrap";
import { OrderModel } from "../../types";
import { InputType } from "reactstrap/types/lib/Input";
import InputGroupWrapper from "./InputGroupWrapper";

type FormFieldProps<T = OrderModel> = RegisterOptions & {
  // if you want to use this component for other models, add type to T
  type?: InputType;
  name: keyof T;
  label: string;
  prepend?: React.ReactNode;
  append?: React.ReactNode;
  register: UseFormRegister<T>;
  errors: FieldErrors<T>;
  required?: boolean; // overwriting RegisterOptions required
  maxLength?: number;
  readOnly?: boolean;
  selectOptions?: { label: string; value: string }[];
};

const FormField: React.FC<FormFieldProps> = ({
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
  selectOptions,
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
          children={selectOptions?.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
          {...rest}
        />
      </InputGroupWrapper>
      {errors[name] && <FormFeedback>{errors[name].message}</FormFeedback>}
    </FormGroup>
  );
};

export default FormField;
