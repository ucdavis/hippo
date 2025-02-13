import { GroupModel, RequestModel } from "../types";

export const getGroupModelFromRequest = (r: RequestModel) => {
  return r.action === "CreateGroup"
    ? ({
        name: r.data.name,
        displayName: r.data.displayName,
        admins: [
          {
            name: r.requesterName,
            email: r.requesterEmail,
          },
        ],
      } as GroupModel)
    : r.groupModel;
};
