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
      readOnly={!canEditConditions}
      disabled={!canEditConditions}
      {...props}
    />
  );
};

export default OrderFormField;
