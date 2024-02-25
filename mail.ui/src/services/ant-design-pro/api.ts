// @ts-ignore
/* eslint-disable */
import { request } from '@umijs/max';
import { parse } from 'querystring';

export async function addHeaderToken(options?: { [key: string]: any }) {
  if(options == null) {
    options = {};
  }
  var token = localStorage.getItem('token');
  if(token != null) {
    options = {
      ...options,
      headers: {
        "Authorization": token,
      }
    };
  }
  return options;
}

/** 获取当前的用户 GET /api/currentUser */
export async function currentUser(options?: { [key: string]: any }) {
  options = await addHeaderToken(options);
  return request<{
    data: API.CurrentUser;
  }>('/mail/api/login/currentuser', {
    method: 'GET',
    ...(options || {}),
  });
}

/** 退出登录接口 POST /api/login/outLogin */
export async function outLogin(options?: { [key: string]: any }) {
  options = await addHeaderToken(options);
  return request<Record<string, any>>('/mail/api/login/outLogin', {
    method: 'POST',
    ...(options || {}),
  });
}

/** 登录接口 POST /api/login/account */
export async function login(body: API.LoginParams, options?: { [key: string]: any }) {
  return request<API.LoginResult>('/mail/api/login/account', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    data: body,
    ...(options || {}),
  });
}

export async function edit_pwd(options?: { [key: string]: any }) {
  return request<Record<string, any>>('/mail/api/login/editpwd', {
    method: 'POST',
    data:{
      method: 'POST',
      ...(options || {}),
    }
  });
}

export async function mail_login(options?: { [key: string]: any }) {
  return request<Record<string, any>>('/mail/api/login/account', {
    method: 'POST',
    data:{
      method: 'POST',
      ...(options || {}),
    }
  });
}

export async function mail_get_settings(options?: { [key: string]: any }) {
  return request<Record<string, any>>('/mail/api/mail/getsettings', {
    method: 'GET',
    data:{
      method: 'GET',
      ...(options || {}),
    }
  });
}

export async function mail_save_settings(options?: { [key: string]: any }) {
  return request<Record<string, any>>('/mail/api/mail/updatesettings', {
    method: 'POST',
    data:{
      method: 'POST',
      ...(options || {}),
    }
  });
}

export async function mail_schedule_task_list(options?: { [key: string]: any }) {
  return request<Record<string, any>>('/mail/api/mail/gettasklist', {
    method: 'POST',
    data:{
      method: 'POST',
      ...(options || {}),
    }
  });
}

export async function mail_schedule_task_delete(options?: { [key: string]: any }) {
  return request<Record<string, any>>('/mail/api/mail/removetaskbyname', {
    method: 'POST',
    data:{
      method: 'POST',
      ...(options || {}),
    }
  });
}

export async function mail_schedule_task_detail(options?: { [key: string]: any }) {
  var taskName = parse(window.location.href.split('?')[1]).id;
  if(options == null) {
    options = {};
  }
  options['batch_id'] = taskName;
  return request<Record<string, any>>('/mail/api/mail/gettaskdetails', {
    method: 'POST',
    data:{
      method: 'POST',
      ...(options || {}),
    }
  });
}

export async function mail_schedule_task_detail_delete(options?: { [key: string]: any }) {
  return request<Record<string, any>>('/mail/api/mail/removeschedulemail', {
    method: 'POST',
    data:{
      method: 'POST',
      ...(options || {}),
    }
  });
}

