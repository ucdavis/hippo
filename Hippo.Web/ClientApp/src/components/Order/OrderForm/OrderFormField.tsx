import React from "react";
import { useFormContext } from "react-hook-form";
import HipFormField from "../../../Shared/Form/HipFormField";
import { HipFormFieldOptions } from "../../../Shared/Form/formTypes";
import { OrderModel } from "../../../types";

type OrderFormFieldProps = HipFormFieldOptions<OrderModel> & {
  canEditConditions: boolean;
};

const OrderFormField: React.FC<OrderFormFieldProps> = ({
  canEditConditions,
  size,
  ...props
}) => {
  const {
    register,
    getValues,
    formState: { errors },
  } = useFormContext<OrderModel>();

  return (
    <HipFormField
      register={register}
      getValues={getValues}
      error={errors[props.name]}
      readOnly={!canEditConditions}
      disabled={!canEditConditions}
      size={size ?? canEditConditions ? "md" : "sm"}
      {...props}
    />
  );
};

export default OrderFormField;
