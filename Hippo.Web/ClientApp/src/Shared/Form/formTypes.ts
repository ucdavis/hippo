import { InputProps } from "react-bootstrap-typeahead/types/types";
import {
  ArrayPath,
  Control,
  FieldError,
  FieldErrors,
  FieldPath,
  RegisterOptions,
  UseFormRegister,
} from "react-hook-form";
import { InputType } from "reactstrap/types/lib/Input";

type FormFieldCommonProps<T> = Omit<InputProps, "pattern"> &
  RegisterOptions & {
    type?: InputType;
    label: string;
    inputPrepend?: React.ReactNode;
    inputAppend?: React.ReactNode;
  };

export type HipFormFieldOptions<T> = FormFieldCommonProps<T> & {
  error?: FieldError;
  name: FieldPath<T>;
  size?: "sm" | "md" | "lg";
  colSize?: number;
  feedback?: React.ReactNode;
};

export type HipFormFieldProps<T> = HipFormFieldOptions<T> & {
  register: UseFormRegister<T>;
};

export type HipFormFieldArrayProps<T> = FormFieldCommonProps<T> & {
  arrayName: ArrayPath<T>;
  errors: FieldErrors<T>;
  register: UseFormRegister<T>;
  control: Control<T>;
};
