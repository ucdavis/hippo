import React from "react";
import { FieldErrors, RegisterOptions, UseFormRegister } from "react-hook-form";
import { FormFeedback, FormGroup, Input, Label } from "reactstrap";
import { OrderModel } from "../../types";
import { InputType } from "reactstrap/types/lib/Input";

type FormFieldProps<T = OrderModel> = RegisterOptions & {
  // if you want to use this component for other models, add type to T
  type?: InputType;
  name: keyof T;
  label: string;
  register: UseFormRegister<T>;
  errors: FieldErrors<T>;
  readOnly?: boolean;
};

const FormField: React.FC<FormFieldProps> = ({
  type = "text",
  name,
  label,
  register,
  errors,
  readOnly = false,
  ...options
}) => {
  console.log(errors);
  return (
    <FormGroup>
      <Label for={`field-${name}`}>{label}</Label>
      <Input
        id={`field-${name}`}
        {...register(name, options)}
        type={type}
        invalid={!!errors[name]}
        readOnly={readOnly}
      />
      {errors[name] && <FormFeedback>{errors[name].message}</FormFeedback>}
    </FormGroup>
  );
};

export default FormField;
