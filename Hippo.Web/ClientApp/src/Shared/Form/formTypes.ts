import {
  ArrayPath,
  Control,
  FieldPath,
  RegisterOptions,
  UseFormRegister,
} from "react-hook-form";
import { InputType } from "reactstrap/types/lib/Input";

/**
 * Props that control the look of the input itself
 */
export type ReactstrapFieldProps = {
  type?: InputType;
  label: string;
  inputPrepend?: React.ReactNode;
  inputAppend?: React.ReactNode;
  required?: boolean; // overwriting RegisterOptions required tp be bool
  maxLength?: number; // overwriting RegisterOptions maxLength tp be number
  readOnly?: boolean;
};

export type FormFieldProps<T> = RegisterOptions &
  ReactstrapFieldProps & {
    register: UseFormRegister<T>;
    error: string;
    name: FieldPath<T>;
  };

export type FormFieldArrayProps<T> = {
  name: ArrayPath<T>;
  control: Control<T>;
};
