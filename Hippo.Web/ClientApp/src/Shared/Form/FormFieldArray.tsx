import React from "react";
import { FormFeedback, FormGroup, Input, Label } from "reactstrap";
import InputGroupWrapper from "./InputGroupWrapper";
import { FormFieldFullProps, FormFieldHookProps } from "./formTypes";
import { Control, FieldArrayWithId } from "react-hook-form";

type FormFieldArrayFullProps<T> = FormFieldFullProps<T> & {
  control: Control<T>;
  fields: FieldArrayWithId<T>[];
};

const FormFieldArray = <T extends Record<string, any>>({
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
}: FormFieldArrayFullProps<T>) => {

  return (

  );
};

export default FormFieldArray;
