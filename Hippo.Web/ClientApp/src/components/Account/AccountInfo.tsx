import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import { GroupModel, IRouteParams } from "../../types";
import { GroupInfo } from "../Group/GroupInfo";
import { GroupLookup } from "../Group/GroupLookup";
import { CardColumns } from "reactstrap";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../../util/Notifications";
import { authenticatedFetch } from "../../util/api";

export const AccountInfo = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [context] = useContext(AppContext);
  const { cluster } = useParams<IRouteParams>();

  const currentGroups = useMemo(
    () =>
      context.accounts.filter((a) => a.cluster === cluster)[0]?.groups ?? [],
    [context.accounts, cluster]
  );

  const [groups, setGroups] = useState<GroupModel[]>([]);
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/group/groups`);

      if (response.ok) {
        setGroups(
          ((await response.json()) as GroupModel[]).filter(
            (g) => !currentGroups.some((cg) => cg.id === g.id)
          )
        );
      } else {
        alert("Error fetching groups");
      }
    };

    fetchGroups();
  }, [cluster, currentGroups]);

  const [getGroupConfirmation] = useConfirmationDialog<GroupModel>(
    {
      title: "Request Access to Group",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <GroupLookup
                  setSelection={(selection) => setReturn(selection)}
                  options={groups}
                />
              </div>
            </div>
          </div>
        );
      },
      canConfirm: (returnValue) => returnValue !== undefined,
    },
    [groups]
  );

  const handleRequestAccess = useCallback(async () => {
    const [confirmed, group] = await getGroupConfirmation();
    if (confirmed) {
      const request = fetch(`/api/${cluster}/group/request/${group.id}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
      });

      setNotification(request, "Request sent", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
    }
  }, [cluster, getGroupConfirmation, setNotification]);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8 text-center">
        <p>
          Welcome {context.user.detail.firstName}. Your account is registered
          with the following group(s):
        </p>
        <CardColumns>
          {currentGroups.map((g, i) => (
            <GroupInfo group={g} key={i} />
          ))}
        </CardColumns>
        <br />
        <p>
          <button
            disabled={notification.pending}
            onClick={() => handleRequestAccess()}
            className="btn btn-primary"
          >
            Request Access to Additional Groups
          </button>
        </p>
        <p>
          Documentation about this cluster can be found{" "}
          <a
            href="https://wiki.cse.ucdavis.edu/support/systems/farm"
            target={"_blank"}
            rel="noreferrer"
          >
            at the UC Davis Farm Wiki
          </a>
          .
        </p>
      </div>
    </div>
  );
};
