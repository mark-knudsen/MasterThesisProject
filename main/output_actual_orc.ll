; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_name_age = type { ptr, i64 }
%array = type { i64, i64, ptr }

@__i = external global i64, align 8
@str = private unnamed_addr constant [6 x i8] c"harry\00", align 1
@str.1 = private unnamed_addr constant [6 x i8] c"Billy\00", align 1
@str.2 = private unnamed_addr constant [6 x i8] c"harry\00", align 1
@str.3 = private unnamed_addr constant [6 x i8] c"Billy\00", align 1
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@item = external global ptr, align 8
@fmt_str = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

define ptr @main_1() {
entry:
  store i64 0, ptr @__i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__i_load = load i64, ptr @__i, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 32)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 2, ptr %len_ptr, align 8
  store i64 2, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %record_ptr = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i64))
  %field_0 = getelementptr %struct_name_age, ptr %record_ptr, i32 0, i32 0
  store ptr @str, ptr %field_0, align 8
  %field_1 = getelementptr %struct_name_age, ptr %record_ptr, i32 0, i32 1
  store i64 12, ptr %field_1, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data_raw, i64 0
  store ptr %record_ptr, ptr %elem_ptr, align 8
  %record_ptr1 = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i64))
  %field_02 = getelementptr %struct_name_age, ptr %record_ptr1, i32 0, i32 0
  store ptr @str.1, ptr %field_02, align 8
  %field_13 = getelementptr %struct_name_age, ptr %record_ptr1, i32 0, i32 1
  store i64 23, ptr %field_13, align 8
  %elem_ptr4 = getelementptr ptr, ptr %arr_data_raw, i64 1
  store ptr %record_ptr1, ptr %elem_ptr4, align 8
  %len_ptr5 = getelementptr inbounds nuw %array, ptr %arr_header, i32 0, i32 0
  %len = load i64, ptr %len_ptr5, align 8
  %icmp_tmp = icmp slt i64 %__i_load, %len
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %arr_header6 = call ptr @malloc(i64 24)
  %arr_data_raw7 = call ptr @malloc(i64 32)
  %len_ptr8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header6, i32 0, i32 0
  %cap_ptr9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header6, i32 0, i32 1
  %data_field_ptr10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header6, i32 0, i32 2
  store i64 2, ptr %len_ptr8, align 8
  store i64 2, ptr %cap_ptr9, align 8
  store ptr %arr_data_raw7, ptr %data_field_ptr10, align 8
  %record_ptr11 = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i64))
  %field_012 = getelementptr %struct_name_age, ptr %record_ptr11, i32 0, i32 0
  store ptr @str.2, ptr %field_012, align 8
  %field_113 = getelementptr %struct_name_age, ptr %record_ptr11, i32 0, i32 1
  store i64 12, ptr %field_113, align 8
  %elem_ptr14 = getelementptr ptr, ptr %arr_data_raw7, i64 0
  store ptr %record_ptr11, ptr %elem_ptr14, align 8
  %record_ptr15 = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i64))
  %field_016 = getelementptr %struct_name_age, ptr %record_ptr15, i32 0, i32 0
  store ptr @str.3, ptr %field_016, align 8
  %field_117 = getelementptr %struct_name_age, ptr %record_ptr15, i32 0, i32 1
  store i64 23, ptr %field_117, align 8
  %elem_ptr18 = getelementptr ptr, ptr %arr_data_raw7, i64 1
  store ptr %record_ptr15, ptr %elem_ptr18, align 8
  %__i_load19 = load i64, ptr @__i, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header6, i32 0, i32 0
  %data_field_ptr20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header6, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_field_ptr20, align 8
  %array_len = load i64, ptr %len_field_ptr, align 8
  %index_is_neg = icmp slt i64 %__i_load19, 0
  %index_rel = add i64 %__i_load19, %array_len
  %resolved_index = select i1 %index_is_neg, i64 %index_rel, i64 %__i_load19
  %is_neg = icmp slt i64 %resolved_index, 0
  %is_too_big = icmp sge i64 %resolved_index, %array_len
  %is_invalid = or i1 %is_neg, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %bounds.ok
  %x_load = load i64, ptr @__i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr22 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr22, align 8
  ret ptr %runtime_obj

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %elem_ptr21 = getelementptr ptr, ptr %data_ptr, i64 %resolved_index
  %loaded_val = load ptr, ptr %elem_ptr21, align 8
  store ptr %loaded_val, ptr @item, align 8
  %item_load = load ptr, ptr @item, align 8
  %ptr_name = getelementptr %struct_name_age, ptr %item_load, i32 0, i32 0
  %val_name = load ptr, ptr %ptr_name, align 8
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_str, ptr %val_name)
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
