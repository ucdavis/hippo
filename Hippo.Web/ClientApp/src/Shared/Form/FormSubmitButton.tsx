import React from "react";
import { Button } from "reactstrap";

interface FormSubmitButtonProps {
  isDirty: boolean;
  isSubmitting: boolean;
  isSubmitSuccessful: boolean;
  submitError?: string;
}

const FormSubmitButton: React.FC<FormSubmitButtonProps> = ({
  isDirty,
  isSubmitting,
  isSubmitSuccessful,
  submitError,
}) => {
  const shouldDisable = !isDirty || isSubmitting || isSubmitSuccessful;

  return (
    <>
      <Button
        type="submit"
        block={true}
        outline={!shouldDisable} // for styling
        color="primary"
        disabled={shouldDisable}
      >
        {isSubmitting
          ? "Submitting..."
          : isSubmitSuccessful
            ? "Submitted!"
            : "Submit"}
      </Button>
      {submitError && <span className="text-danger">{submitError}</span>}
    </>
  );
};

export default FormSubmitButton;
