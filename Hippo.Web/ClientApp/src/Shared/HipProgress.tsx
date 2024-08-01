import React from "react";
import { Progress } from "reactstrap";

interface HipProgressProps {
  label: string;
  max: number;
  active: boolean;
  inProgress: boolean;
}

const HipProgress: React.FC<HipProgressProps> = ({
  label,
  max,
  active,
  inProgress,
}) => {
  return (
    <Progress
      bar
      max={max}
      value={1}
      color={active ? "primary" : inProgress ? "secondary" : "tertiary"}
      striped={inProgress} // but show stripes for next step status
    >
      {label}
    </Progress>
  );
};

export default HipProgress;
