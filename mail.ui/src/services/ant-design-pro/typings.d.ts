// @ts-ignore
/* eslint-disable */

declare namespace API {
  type CurrentUser = {
    name?: string;
    avatar?: string;
    userid?: string;
    email?: string;
    signature?: string;
    title?: string;
    group?: string;
    tags?: { key?: string; label?: string }[];
    notifyCount?: number;
    unreadCount?: number;
    country?: string;
    access?: string;
    geographic?: {
      province?: { label?: string; key?: string };
      city?: { label?: string; key?: string };
    };
    address?: string;
    phone?: string;
  };

  type LoginResult = {
    status?: string;
    type?: string;
    currentAuthority?: string;
  };

  type PageParams = {
    current?: number;
    pageSize?: number;
  };

  type RuleListItem = {
    key?: number;
    disabled?: boolean;
    href?: string;
    avatar?: string;
    name?: string;
    owner?: string;
    desc?: string;
    callNo?: number;
    status?: number;
    updatedAt?: string;
    createdAt?: string;
    progress?: number;
  };

  type RuleList = {
    data?: RuleListItem[];
    /** 列表的内容总数 */
    total?: number;
    success?: boolean;
  };

  type FakeCaptcha = {
    code?: number;
    status?: string;
  };

  type LoginParams = {
    username?: string;
    password?: string;
    autoLogin?: boolean;
    type?: string;
  };

  type ErrorResponse = {
    /** 业务约定的错误码 */
    errorCode: string;
    /** 业务上的错误信息 */
    errorMessage?: string;
    /** 业务上的请求是否成功 */
    success?: boolean;
  };

  type NoticeIconList = {
    data?: NoticeIconItem[];
    /** 列表的内容总数 */
    total?: number;
    success?: boolean;
  };

  type NoticeIconItemType = 'notification' | 'message' | 'event';

  type NoticeIconItem = {
    id?: string;
    extra?: string;
    key?: string;
    read?: boolean;
    avatar?: string;
    title?: string;
    status?: string;
    datetime?: string;
    description?: string;
    type?: NoticeIconItemType;
  };

  type ScheduleTask = {
    /** 任务名称 */
    Name: string;
    /** 子任务数量 */
    TaskCount?: number;
    /** 未发送，计划中的任务 */
    UnSendCount?: number;
    /** 已发送，计划中的任务 */
    SendCount?: number;
    /** 任务更新时间 */
    UpdatedAt?: string;
    /** 任务创建时间 */
    CreatedAt?: string;
    /** 任务是否已完成 */
    IsFinish?: boolean;
  };

  type ScheduleTaskDetail = {
    /** 索引 */
    Index: number;
    /** 任务名称 */
    BatchId: string;
    /** 邮箱地址 */
    Mail?: string;
    /** 计划发送时间 */
    ScheduleTime?: string;
    /** 发送时昵称 */
    NickName?: string;
    /** 邮件类别 */
    MailType?: number;
    /** 标题 */
    Subject?: string;
    /** 邮件内容 */
    MailBody?: string;
    /** 是否发送 */
    IsSend?: boolean;
    /** 发送时间 */
    LastSend?: string;
  };
}
