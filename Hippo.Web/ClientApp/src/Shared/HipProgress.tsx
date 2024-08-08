import React from "react";
import { Progress, ProgressProps } from "reactstrap";

interface HipProgressProps extends ProgressProps {
  label: string;
  max: number;
  shouldFill: boolean;
  inProgress: boolean;
  animated?: boolean;
}

const HipProgress: React.FC<HipProgressProps> = ({
  label,
  max,
  shouldFill,
  inProgress,
  animated,
  value,
  color,
  ...rest
}) => {
  return (
    <Progress
      bar
      max={max}
      value={value ?? 1}
      color={
        color ??
        (shouldFill ? "primary" : inProgress ? "secondary" : "tertiary")
      }
      striped={inProgress} // but show stripes for next step status
      animated={animated}
      {...rest}
    >
      {label}
    </Progress>
  );
};

export default HipProgress;
