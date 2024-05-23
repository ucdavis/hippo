import React from "react";
import { ButtonProps } from "reactstrap";
import { OrderModel } from "../../types";
import { useFormContext } from "react-hook-form";
import HipButton from "../HipButton";

interface FormSubmitButtonProps extends ButtonProps {}

const FormSubmitButton: React.FC<FormSubmitButtonProps> = ({ ...props }) => {
  const {
    formState: { isDirty, isSubmitting, isSubmitSuccessful, errors },
  } = useFormContext<OrderModel>();

  const shouldDisable = !isDirty || isSubmitting || isSubmitSuccessful;

  return (
    <>
      <HipButton type="submit" disabled={shouldDisable} {...props} block={true}>
        {isSubmitting
          ? "Submitting..."
          : isSubmitSuccessful
            ? "Submitted!"
            : "Submit"}
      </HipButton>
      {errors.root && (
        <span className="text-danger">{errors.root?.message}</span>
      )}
    </>
  );
};

export default FormSubmitButton;
