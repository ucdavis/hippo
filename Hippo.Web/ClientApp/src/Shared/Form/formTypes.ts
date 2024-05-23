import { FieldPath, RegisterOptions, UseFormRegister } from "react-hook-form";
import { InputType } from "reactstrap/types/lib/Input";

export type FormFieldProps<T> = {
  type?: InputType;
  name: FieldPath<T>;
  label: string;
  prepend?: React.ReactNode;
  append?: React.ReactNode;
  required?: boolean; // overwriting RegisterOptions required tp be bool
  maxLength?: number; // overwriting RegisterOptions maxLength tp be number
  readOnly?: boolean;
};

export type FormFieldHookProps<T> = {
  register: UseFormRegister<T>;
  error: string;
  name: FieldPath<T>;
};

export type FormFieldFullProps<T> = RegisterOptions &
  FormFieldProps<T> &
  FormFieldHookProps<T>;
