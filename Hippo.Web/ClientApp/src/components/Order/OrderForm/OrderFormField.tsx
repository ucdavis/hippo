import React from "react";
import { useFormContext } from "react-hook-form";
import HipFormField from "../../../Shared/Form/HipFormField";
import { HipFormFieldProps } from "../../../Shared/Form/formTypes";
import { OrderModel } from "../../../types";

type OrderFormFieldProps = HipFormFieldProps<OrderModel> & {
  canEditConditions: boolean;
  disabled?: never;
  readOnly?: never;
};

const OrderFormField: React.FC<OrderFormFieldProps> = ({
  canEditConditions,
  readOnly,
  disabled,
  ...props
}) => {
  const {
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();

  return (
    <HipFormField
      register={register}
      error={errors[props.name]}
      {...props}
      readOnly={!canEditConditions}
      disabled={!canEditConditions}
    />
  );
};

export default OrderFormField;
