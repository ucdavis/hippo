import React from "react";
import { ButtonProps } from "reactstrap";
import { OrderModel } from "../../types";
import { useFormContext } from "react-hook-form";
import HipButton from "../HipButton";

interface HipFormSubmitButtonProps extends ButtonProps {}

const HipFormSubmitButton: React.FC<HipFormSubmitButtonProps> = ({
  ...props
}) => {
  const {
    formState: {
      isDirty,
      isSubmitting,
      //isSubmitSuccessful,
      errors,
    },
  } = useFormContext<OrderModel>();

  const isValid = Object.keys(errors).length === 0;

  // @laholstege TODO: isSubmitSuccessful doesn't work as expected, because CreateOrder doesn't throw an error
  // so the form doesn't know about it and an error is not set and isSubmitSuccessful is always true
  // however, if an error IS thrown, the root error gets set but never cleared, so isValid is always false
  // for known errors (like duplicated chart strings) we should validate on client
  // but come up with a general pattern for handling unexpected errors. prob just let the toast handle it
  const shouldDisable = !isDirty || isSubmitting || !isValid; // || isSubmitSuccessful;

  return (
    <>
      <HipButton type="submit" disabled={shouldDisable} {...props} block={true}>
        {isSubmitting
          ? "Submitting..."
          : // : isSubmitSuccessful
            //   ? "Submitted!"
            "Submit"}
      </HipButton>
      {errors.root && (
        <span className="text-danger">{errors.root?.message}</span>
      )}
    </>
  );
};

export default HipFormSubmitButton;
