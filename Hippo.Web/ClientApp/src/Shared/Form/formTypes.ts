import {
  ArrayPath,
  Control,
  FieldErrors,
  FieldPath,
  RegisterOptions,
  UseFormRegister,
} from "react-hook-form";
import { InputType } from "reactstrap/types/lib/Input";

export type FormFieldInputProps = {
  type?: InputType;
  label: string;
  inputPrepend?: React.ReactNode;
  inputAppend?: React.ReactNode;
  required?: boolean; // overwriting RegisterOptions required tp be bool
  maxLength?: number; // overwriting RegisterOptions maxLength tp be number
  readOnly?: boolean;
};

export type FormFieldProps<T> = RegisterOptions &
  FormFieldInputProps & {
    register: UseFormRegister<T>;
    error: string;
    name: FieldPath<T>;
  };

export type FormFieldArrayProps<T> = {
  arrayName: ArrayPath<T>;
  errors: FieldErrors<T>;
  register: UseFormRegister<T>;
  control: Control<T>;
};