import React from "react";
import { OrderModel } from "../../types";
import FormField from "../../Shared/Form/FormField";
import { FieldPath, useFormContext } from "react-hook-form";
import { ReactstrapFieldProps } from "../../Shared/Form/formTypes";

type OrderFormFieldProps = ReactstrapFieldProps & {
  name: FieldPath<OrderModel>;
};

const OrderFormField: React.FC<OrderFormFieldProps> = ({ ...props }) => {
  const {
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();

  return (
    <FormField register={register} error={errors[props.name]} {...props} />
  );
};

export default OrderFormField;
