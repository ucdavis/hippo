import { GroupModel } from "../../types";
import { UncontrolledTooltip } from "reactstrap";
import { GroupInfo } from "./GroupInfo";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import GroupDetails from "./GroupDetails";

interface Props {
  group: GroupModel;
  id?: string;
  showDisplayName?: boolean;
}

export const GroupNameWithTooltip = ({
  group,
  id,
  showDisplayName: useDisplayName = true,
}: Props) => {
  const target = `groupName_${group.id}_${id ?? ""}`;
  const [showDetails] = useConfirmationDialog(
    {
      title: "View Group Details",
      message: () => {
        return <GroupDetails group={group} />;
      },
      buttons: ["OK"],
    },
    [group],
  );
  return (
    <span className="dotted-underline">
      <span id={target} style={{ whiteSpace: "nowrap" }}>
        {useDisplayName ? group.displayName : group.name}
      </span>
      <UncontrolledTooltip
        placement="left"
        style={{ backgroundColor: "rgb(233, 226, 237)" }}
        target={target}
        autohide={false}
      >
        <GroupInfo group={group} showDetails={() => showDetails()} />
      </UncontrolledTooltip>
    </span>
  );
};
