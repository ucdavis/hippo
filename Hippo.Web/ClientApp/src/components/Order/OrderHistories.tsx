import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import { HistoryTable } from "./HistoryTable";

export const OrderHistories: React.FC = () => {
  // RH TODO: get history info here to display name?
  return (
    <HipMainWrapper>
      <HipTitle title="" subtitle="Details" />
      <HipBody>
        <HistoryTable
          numberOfRows={1000}
          showLinkToAll={false}
          historyCount={0}
        />
      </HipBody>
    </HipMainWrapper>
  );
};
